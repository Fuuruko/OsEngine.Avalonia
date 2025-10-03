using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers;

public partial class BaseServer
{
    /// <summary>
    /// implementation of connection to the API
    /// </summary>
    public IServerRealization ServerRealization
    {
        set
        {
            _serverConnectStatus = ServerConnectStatus.Disconnect;
            field = value;
            field.NewTradesEvent += ServerRealization_NewTradesEvent;
            field.ConnectEvent += ServerRealization_Connected;
            field.DisconnectEvent += ServerRealization_Disconnected;
            field.MarketDepthEvent += ServerRealization_MarketDepthEvent;
            field.MyOrderEvent += ServerRealization_MyOrderEvent;
            field.MyTradeEvent += ServerRealization_MyTradeEvent;
            field.PortfolioEvent += ServerRealization_PortfolioEvent;
            field.SecurityEvent += ServerRealization_SecurityEvent;
            field.LogMessageEvent += OnLogRecieved;

            field.NewsEvent += _newsToSend.Enqueue;

            field.AdditionalMarketDataEvent += ServerRealization_AdditionalMarketDataEvent;


            _namePostfix.ValueChanged += () => Name = ServerName + _namePostfix.Value;

            if (Permissions.SupportsProxyForMultipleServers)
            {
                Inputs.Add(new Input.Options("Proxy type", ["None", "Auto", "Manual"]));
                Inputs.Add(new Input.String("Proxy", ""));
            }


            _tradesStorage = new ServerTradesStorage(this)
            {
                IsSaveTrades = _isKeepTrades,
                DaysToLoad = _uploadTradesDaysNumber,
            };
            _tradesStorage.LogMessageEvent += OnLogRecieved;

            AllTrades = _tradesStorage.LoadTrades();

            _candleStorage = new ServerCandleStorage(this)
            {
                IsSaveCandles = _isKeepCandles,
                SaveCandlesNumber = _keepCandlesNumber,
            };
            _candleStorage.LogMessageEvent += OnLogRecieved;

            // FIX:
            // Logs = new Logs(ServerNameUnique + "Server", StartProgram.IsOsTrader);
            // Logs.Listen(this);


            // NOTE: Task eat cpu start only on connection and cancel on USER disconnection
            // Use ManualResetEventSlim if not connected
            Task.Run(ExecutorOrdersThreadArea);
            Task.Run(PrimeThreadArea);
            Task.Run(SenderThreadArea);
            Task.Run(MyTradesBeepThread);

            if (Permissions.SupportsCheckDataFeedLogic)
            {
                _checkDataFlowIsOn = true;
                Task.Run(CheckDataFlowThread);
            }

            _ordersHub = new OrdersHub(this);
            _ordersHub.LogMessageEvent += OnLogRecieved;
            _ordersHub.GetAllActiveOrdersOnReconnectEvent += _ordersHub_GetAllActiveOrdersOnReconnectEvent;
            _ordersHub.ActiveStateOrderCheckStatusEvent += ActiveStateOrderCheckStatusEvent;

            // FIX:
            // ComparePositionsModule = new ComparePositionsModule(this);
            // ComparePositionsModule.LogMessageEvent += SendLogMessage;
        }
        get;
    }

    /// <summary>
    /// new trade event from ServerRealization
    /// </summary>
    private void ServerRealization_NewTradesEvent(Trade trade)
    {
        try
        {
            if (trade == null || trade.Price == 0) { return; }

            ServerTime = trade.Time;

            if (_needToLoadBidAskInTrades2)
            {
                BathTradeMarketDepthData(trade);
            }

            lock (_newTradesLocker)
            {
                // save / сохраняем
                if (AllTrades == null)
                {
                    AllTrades = new List<Trade>[1];
                    AllTrades[0] = [trade];
                    return;
                }

                // sort trades by storages / сортируем сделки по хранилищам
                List<Trade> myList = null;
                bool isSave = false;
                for (int i = 0; i < AllTrades.Length; i++)
                {
                    List<Trade> curList = AllTrades[i];

                    if (curList == null
                            || curList.Count == 0
                            || curList[0] == null)
                    {
                        continue;
                    }

                    if (curList[0].SecurityNameCode != trade.SecurityNameCode)
                    {
                        continue;
                    }

                    if (trade.Time < curList[^1].Time)
                    {
                        return;
                    }

                    if (_isUpdateOnlyNewPriceTrades)
                    {
                        Trade lastTrade = curList[^1];

                        if (lastTrade == null
                                || lastTrade.Price == trade.Price)
                        {
                            return;
                        }
                    }

                    curList.Add(trade);
                    myList = curList;
                    isSave = true;
                    break;
                }

                if (isSave == false)
                {
                    // there is no storage for instrument / хранилища для инструмента нет
                    List<Trade>[] allTradesNew = new List<Trade>[AllTrades.Length + 1];
                    for (int i = 0; i < AllTrades.Length; i++)
                    {
                        allTradesNew[i] = AllTrades[i];
                    }

                    allTradesNew[^1] = [trade];
                    myList = allTradesNew[^1];
                    AllTrades = allTradesNew;
                }

                _tradesToSend.Enqueue(myList);

            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// alert message from client that connection is established
    /// </summary>
    private void ServerRealization_Connected()
    {
        OnLogRecieved(OsLocalization.Market.Message6, LogMessageType.System);
        ServerStatus = ServerConnectStatus.Connect;
    }

    /// <summary>
    /// client connection has broken
    /// </summary>
    private void ServerRealization_Disconnected()
    {
        if (ServerStatus == ServerConnectStatus.Disconnect)
        {
            return;
        }
        OnLogRecieved(OsLocalization.Market.Message12, LogMessageType.System);
        ServerStatus = ServerConnectStatus.Disconnect;

        if (ServerRealization.ServerStatus != ServerConnectStatus.Disconnect)
        {
            ServerRealization.ServerStatus = ServerConnectStatus.Disconnect;
        }

        NeedToReconnectEvent?.Invoke();
    }

    /// <summary>
    /// portfolio updated
    /// </summary>
    private void ServerRealization_PortfolioEvent(List<Portfolio> portf)
    {
        try
        {
            for (int i = 0; i < portf.Count; i++)
            {
                if (portf[i].ServerType == ServerType.None)
                {
                    portf[i].ServerType = ServerType;
                }

                if (string.IsNullOrEmpty(portf[i].ServerUniqueName))
                {
                    portf[i].ServerUniqueName = ServerNameAndPrefix;
                }

                if(portf[i].ServerUniqueName != ServerNameAndPrefix)
                {
                    portf[i].ServerUniqueName = ServerNameAndPrefix;
                }

                Portfolio curPortfolio = Portfolios.Find(p => p.Number == portf[i].Number);

                if (curPortfolio == null)
                {
                    bool isInArray = false;

                    for (int i2 = 0; i2 < Portfolios.Count; i2++)
                    {
                        if (Portfolios[i2].Number[0] > portf[i].Number[0])
                        {
                            Portfolios.Insert(i2, portf[i]);
                            curPortfolio = portf[i];
                            isInArray = true;
                            break;
                        }
                    }

                    if (isInArray == false)
                    {
                        Portfolios.Add(portf[i]);
                        curPortfolio = portf[i];
                    }
                }

                curPortfolio.UnrealizedPnl = portf[i].UnrealizedPnl;
                curPortfolio.ValueBegin = portf[i].ValueBegin;
                curPortfolio.ValueCurrent = portf[i].ValueCurrent;
                curPortfolio.ValueBlocked = portf[i].ValueBlocked;

                var positions = portf[i].GetPositionOnBoard();

                if (positions != null)
                {
                    for (int i2 = 0; i2 < positions.Count; i2++)
                    {
                        curPortfolio.SetNewPosition(positions[i2]);
                    }
                }
            }

            _portfolioToSend.Enqueue(Portfolios);
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// new depth event
    /// </summary>
    private void ServerRealization_MarketDepthEvent(MarketDepth myDepth)
    {
        try
        {
            if (myDepth.Time == DateTime.MinValue)
            {
                myDepth.Time = ServerTime;
            }
            else
            {
                ServerTime = myDepth.Time;
            }

            if ((myDepth.Asks == null ||
                        myDepth.Asks.Count == 0)
                    &&
                    (myDepth.Bids == null ||
                     myDepth.Bids.Count == 0))
            {
                return;
            }

            if (myDepth.SecurityNameCode == "LQDT")
            {

            }

            TrySendMarketDepthEvent(myDepth);
            TrySendBidAsk(myDepth);

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// incoming order from system
    /// </summary>
    private void ServerRealization_MyOrderEvent(Order myOrder)
    {
        if (myOrder.TimeCallBack == DateTime.MinValue)
        {
            myOrder.TimeCallBack = ServerTime;
        }
        if (myOrder.TimeCreate == DateTime.MinValue)
        {
            myOrder.TimeCreate = ServerTime;
        }
        if (myOrder.State == OrderStateType.Done &&
                myOrder.TimeDone == DateTime.MinValue)
        {
            myOrder.TimeDone = myOrder.TimeCallBack;
        }
        if (myOrder.State == OrderStateType.Cancel &&
                myOrder.TimeDone == DateTime.MinValue)
        {
            myOrder.TimeCancel = myOrder.TimeCallBack;
        }

        myOrder.ServerType = ServerType;

        _ordersToSend.Enqueue(myOrder);
    }

    /// <summary>
    /// my trades incoming from IServerRealization
    /// </summary>
    private void ServerRealization_MyTradeEvent(MyTrade trade)
    {
        if (trade.Time == DateTime.MinValue)
        {
            trade.Time = ServerTime;
        }

        _myTradesToSend.Enqueue(trade);
    }

    /// <summary>
    /// security list updated
    /// </summary>
    private void ServerRealization_SecurityEvent(List<Security> securities)
    {
        try
        {
            if (securities == null
                    || securities.Count == 0)
            {
                return;
            }

            TryUpdateSecuritiesUserSettings(securities);

            if (Securities.Count == 0
                    && securities.Count > 5000)
            {
                Securities = securities;
                _securitiesToSend.Enqueue(Securities);

                return;
            }

            Securities ??= [];

            for (int i = 0; i < securities.Count; i++)
            {
                if (securities[i] == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(securities[i].NameId))
                {
                    OnLogRecieved(OsLocalization.Market.Message13, LogMessageType.Error);
                    continue;
                }
                if (string.IsNullOrEmpty(securities[i].Name))
                {
                    OnLogRecieved(OsLocalization.Market.Message98, LogMessageType.Error);
                    continue;
                }

                if (Securities.Find(s =>
                            s != null &&
                            s.NameId == securities[i].NameId &&
                            s.Name == securities[i].Name &&
                            s.NameClass == securities[i].NameClass) == null)
                {
                    bool isInArray = false;

                    for (int i2 = 0; i2 < Securities.Count; i2++)
                    {
                        if (Securities[i2].Name[0] > securities[i].Name[0])
                        {
                            Securities.Insert(i2, securities[i]);
                            isInArray = true;
                            break;
                        }
                    }

                    if (isInArray == false)
                    {
                        Securities.Add(securities[i]);
                    }
                }
            }

            _securitiesToSend.Enqueue(Securities);
        }
        catch (Exception ex)
        {
            OnLogRecieved("AServer Error. _serverRealization_SecurityEvent  " + ex.ToString(), LogMessageType.Error);
        }
    }

    private void ServerRealization_AdditionalMarketDataEvent(OptionMarketDataForConnector obj)
    {
        try
        {
            if (string.IsNullOrEmpty(obj?.SecurityName)) { return; }

            _additionalMarketDataToSend.Enqueue(obj);
        }
        catch (Exception ex)
        {
            OnLogRecieved(ex.ToString(), LogMessageType.Error);
        }
    }
}
