using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;
using OsEngine.Models.Utils;

namespace OsEngine.Models.Market.Servers;

public abstract partial class BaseServer : IServer, ILog
{
    protected virtual string ServerName { get; }
    public string Name { get; set; }
    // NOTE: Maybe should not be public
    public Guid GUID = Guid.NewGuid();

    public BaseServer()
    {
        SetupInputs();
    }

    #region Start / Stop server - user direction

    /// <summary>
    /// server type
    /// </summary>
    public ServerType ServerType => ServerRealization.ServerType;

    public int ServerNum;

    public string ServerPrefix
    {
        get => _serverPrefix;
        set
        {
            if (value == _serverPrefix)
            {
                return;
            }

            _serverPrefix = value;
        }
    }
    private string _serverPrefix;

    public string ServerNameUnique
    {
        get
        {
            string result = ServerType.ToString();

            if (ServerNum == 0)
            {
                return result;
            }

            result = result + "_" + ServerNum;

            return result;
        }
    }

    public string ServerNameAndPrefix
    {
        get
        {
            if (ServerNum == 0
                    || string.IsNullOrEmpty(ServerPrefix))
            {
                return ServerNameUnique;
            }

            string result = ServerNameUnique + "_" + ServerPrefix;

            return result;
        }
    }

    private bool _checkDataFlowIsOn;


    #endregion

    #region Thread 2. Data forwarding operations

    /// <summary>
    /// workplace of the thread sending data to the top
    /// </summary>
    private async void SenderThreadArea()
    {
        while (true)
        {
            try
            {
                if (_ordersToSend.TryDequeue(out Order order)
                        && TestValue_CanSendOrdersUp)
                {
                    NewOrderIncomeEvent?.Invoke(order);

                    _ordersHub.SetOrderFromApi(order);

                    // FIX: Probably error here or in next if
                    foreach (MyTrade v in _userTrades)
                    {
                        if (v.NumberOrderParent == order.NumberMarket)
                        {
                            _myTradesToSend.Enqueue(v);
                        }
                    }
                }
                else if (_ordersToSend.IsEmpty
                         && _myTradesToSend.TryDequeue(out MyTrade myTrade)
                         && TestValue_CanSendOrdersUp
                         && TestValue_CanSendMyTradesUp)
                {
                    NewMyTradeEvent?.Invoke(myTrade);

                    _ordersHub.SetUserTradeFromApi(myTrade);

                    // _myTrades.FirstOrDefault(t => t.Number == myTrade.Number)
                    // if (_myTrades.Find(t => t.Number == myTrade.Number) == null)
                    if (_userTrades
                            .LastOrDefault(t => t.Number == myTrade.Number) == null)
                    {
                        if (_userTrades.Count == 1000)
                        {
                            // _myTrades.RemoveAt(0);
                            _userTrades.Dequeue();
                        }
                        _userTrades.Enqueue(myTrade);
                        // _myTrades.Add(myTrade);
                        // _myTrades.AddLast(myTrade);
                    }


                    _needToBeepOnTrade = true;
                }
                else if (_tradesToSend.TryDequeue(out List<Trade> trades))
                {
                    // разбираем всю очередь. Отправляем массивы для каждого инструмента один раз
                    List<List<Trade>> list = [trades];

                    while (_tradesToSend.TryDequeue(out List<Trade> newTrades))
                    {
                        bool isInArray = false;

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i][0].SecurityNameCode == newTrades[0].SecurityNameCode)
                            {
                                list[i] = newTrades;
                                isInArray = true;
                            }
                        }

                        if (isInArray == false)
                        {
                            list.Add(newTrades);
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (_checkDataFlowIsOn)
                        {
                            SecurityFlowTime tradeTime = new()
                            {
                                SecurityName = list[i][0].SecurityNameCode,
                                LastTimeTrade = DateTime.Now
                            };
                            _securitiesFeedFlow.Enqueue(tradeTime);
                        }

                        NewTradeEvent?.Invoke(list[i]);
                    }

                    if (_isClearTrades && AllTrades != null)
                    {
                        for (int i = 0; i < AllTrades.Length; i++)
                        {
                            List<Trade> curTrades = AllTrades[i];

                            if (curTrades.Count > 100)
                            {
                                curTrades = curTrades.GetRange(curTrades.Count - 101, 100);
                                AllTrades[i] = curTrades;
                            }
                        }
                    }
                }
                else if (_portfolioToSend.TryDequeue(out List<Portfolio> portfolio))
                {
                    PortfoliosChangeEvent?.Invoke(portfolio);
                }
                else if (_securitiesToSend.TryDequeue(out List<Security> security))
                {
                    SecuritiesChangeEvent?.Invoke(security);
                }
                else if (_newServerTime.TryDequeue(out DateTime time))
                {
                    TimeServerChangeEvent?.Invoke(ServerTime);
                }
                else if (_candleSeriesToSend.TryDequeue(out CandleSeries series))
                {
                    NewCandleIncomeEvent?.Invoke(series);
                }
                else if (_marketDepthsToSend.TryDequeue(out MarketDepth depth))
                {
                    if (_marketDepthsToSend.Count < 1000)
                    {
                        NewMarketDepthEvent?.Invoke(depth);

                        if (_checkDataFlowIsOn)
                        {
                            SecurityFlowTime tradeTime = new()
                            {
                                SecurityName = depth.SecurityNameCode,
                                LastTimeMarketDepth = DateTime.Now
                            };
                            _securitiesFeedFlow.Enqueue(tradeTime);
                        }
                        continue;
                    }

                    // Копится очередь. ЦП не справляется
                    // Отсылаем на верх по последнему стакану для каждого инструмента
                    // Промежуточные срезы - игнорируем

                    List<MarketDepth> list = [depth];

                    while (_marketDepthsToSend.TryDequeue(out MarketDepth newDepth))
                    {
                        bool isInArray = false;

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].SecurityNameCode == newDepth.SecurityNameCode)
                            {
                                list[i] = newDepth;
                                isInArray = true;
                            }
                        }

                        if (isInArray == false)
                        {
                            list.Add(newDepth);
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (_checkDataFlowIsOn)
                        {
                            SecurityFlowTime tradeTime = new()
                            {
                                SecurityName = list[i].SecurityNameCode,
                                LastTimeMarketDepth = DateTime.Now
                            };
                            _securitiesFeedFlow.Enqueue(tradeTime);
                        }

                        NewMarketDepthEvent?.Invoke(list[i]);
                    }
                }

                else if (_bidAskToSend.TryDequeue(out BidAskSender bidAsk))
                {
                    if (_bidAskToSend.Count < 1000)
                    {
                        NewBidAscIncomeEvent?.Invoke(bidAsk.Bid, bidAsk.Ask, bidAsk.Security);
                        continue;
                    }

                    // Копится очередь. ЦП не справляется
                    // Отсылаем на верх по последнему bid/Ask для каждого инструмента
                    // Промежуточные срезы - игнорируем

                    List<BidAskSender> list = [bidAsk];

                    while (_bidAskToSend.TryDequeue(out BidAskSender newBidAsk))
                    {
                        bool isInArray = false;

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].Security.Name == newBidAsk.Security.Name)
                            {
                                list[i] = newBidAsk;
                                isInArray = true;
                            }
                        }

                        if (isInArray == false)
                        {
                            list.Add(newBidAsk);
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        NewBidAscIncomeEvent?.Invoke(list[i].Bid, list[i].Ask, list[i].Security);
                    }
                }
                else if (_newsToSend.TryDequeue(out News news))
                {
                    NewsEvent?.Invoke(news);
                }
                else if (_additionalMarketDataToSend.TryDequeue(out OptionMarketDataForConnector data))
                {
                    ConvertableMarketData(data);
                }

                else
                {
                    if (MainWindow.ProccesIsWorked == false)
                    {
                        return;
                    }
                    await Task.Delay(1);
                }
            }
            catch (Exception error)
            {
                OnLogRecieved(error.ToString(), LogMessageType.Error);
            }
        }
    }

    /// <summary>
    /// queue of new orders
    /// </summary>
    private ConcurrentQueue<Order> _ordersToSend = new();

    public bool TestValue_CanSendOrdersUp = true;

    public bool TestValue_CanSendMyTradesUp = true;

    /// <summary>
    /// queue of ticks
    /// </summary>
    private ConcurrentQueue<List<Trade>> _tradesToSend = new();

    /// <summary>
    /// queue of new or updated portfolios
    /// </summary>
    private ConcurrentQueue<List<Portfolio>> _portfolioToSend = new();

    /// <summary>
    /// queue of my new trades
    /// </summary>
    private ConcurrentQueue<MyTrade> _myTradesToSend = new();

    /// <summary>
    /// queue of new securities
    /// </summary>
    private ConcurrentQueue<List<Security>> _securitiesToSend = new();

    /// <summary>
    /// queue of new time
    /// </summary>
    private ConcurrentQueue<DateTime> _newServerTime = new();

    /// <summary>
    /// queue of updated candles series
    /// </summary>
    private ConcurrentQueue<CandleSeries> _candleSeriesToSend = new();

    /// <summary>
    /// queue of new depths 
    /// </summary>
    private ConcurrentQueue<MarketDepth> _marketDepthsToSend = new();

    /// <summary>
    /// queue of updated bid and ask by security
    /// </summary>
    private ConcurrentQueue<BidAskSender> _bidAskToSend = new();

    /// <summary>
    /// queue for new news
    /// </summary>
    private ConcurrentQueue<News> _newsToSend = new();

    /// <summary>
    /// queue for Additional Market Data
    /// </summary>
    private ConcurrentQueue<OptionMarketDataForConnector> _additionalMarketDataToSend = new();

    #endregion

    #region Server time

    /// <summary>
    /// server time
    /// </summary>
    public DateTime ServerTime
    {
        get;

        private set
        {
            if (value <= field) { return; }

            field = value;

            // TimeServerChangeEvent?.Invoke(field);
            if (_newServerTime.IsEmpty == true)
            {
                _newServerTime.Enqueue(field);
            }
            ServerRealization.ServerTime = field;
        }
    }

    /// <summary>
    /// server time changed event
    /// </summary>
    public event Action<DateTime> TimeServerChangeEvent;

    #endregion

    #region Portfolios

    /// <summary>
    /// all account in the system
    /// </summary>
    public List<Portfolio> Portfolios { get; } = [];

    /// <summary>
    /// take portfolio by number
    /// </summary>
    public Portfolio GetPortfolioForName(string accountNumber)
    {
        try
        {
            return Portfolios.Find(portfolio => portfolio.Number == accountNumber);
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
            return null;
        }
    }

    /// <summary>
    /// portfolios changed event
    /// </summary>
    public event Action<List<Portfolio>> PortfoliosChangeEvent;

    #endregion


    #region  Subscribe to data

    /// <summary>
    /// master of dowloading candles
    /// </summary>
    private CandleManager _candleManager;

    /// <summary>
    /// object for accessing candle storage in the file system
    /// </summary>
    private ServerCandleStorage _candleStorage;

    /// <summary>
    /// candles series changed
    /// </summary>
    /// NOTE: Move to candleManager
    private void _candleManager_CandleUpdateEvent(CandleSeries series)
    {
        // NOTE: its onetime thing should not be in event
        if (series.IsMergedByCandlesFromFile == false)
        {
            series.IsMergedByCandlesFromFile = true;

            if (_isKeepCandles.Value)
            {
                List<Candle> candles = _candleStorage.GetCandles(series.Specification, _keepCandlesNumber.Value);
                series.CandlesAll = series.CandlesAll.Merge(candles);
            }
        }

        // NOTE: its onetime thing should not be in event
        if (series.IsMergedByTradesFromFile == false)
        {
            series.IsMergedByTradesFromFile = true;

            if (_isKeepTrades.Value
                && series.TimeFrameBuilder.SaveTradesInCandles)
            {
                List<Trade> trades = GetAllTradesToSecurity(series.Security);

                if (trades != null && trades.Count > 0)
                {
                    series.LoadTradesInCandles(trades);
                }
            }
        }

        // NOTE: Delete if exceed some threashold
        if (_isClearCandles.Value
                && series.CandlesAll.Count > _keepCandlesNumber.Value
                && ServerTime.Minute % 15 == 0
                && ServerTime.Second == 0
           )
        {
            series.CandlesAll.RemoveRange(0, series.CandlesAll.Count - 1 - _keepCandlesNumber.Value);
        }

        _candleSeriesToSend.Enqueue(series);
    }

    private object _lockerStartNews = new();

    /// <summary>
    /// subscribe to news
    /// </summary>
    public bool SubscribeNews()
    {
        // NOTE: Remove Locker
        lock (_lockerStartNews)
        {
            try
            {
                if (Portfolios.Count > 0 || Securities == null)
                {
                    return false;
                }

                if (LastStartServerTime != DateTime.MinValue &&
                        LastStartServerTime.AddSeconds(10) > DateTime.Now)
                {
                    return false;
                }

                if (ServerStatus != ServerConnectStatus.Connect)
                {
                    return false;
                }

                if (Permissions?.IsNewsServer != true)
                {
                    OnLogRecieved(ServerType + " Aserver. News Subscribe method error. No permission on News in Server", LogMessageType.Error);
                    // NOTE: true?
                    return true;
                }

                return ServerRealization.SubscribeNews();
            }
            catch (Exception ex)
            {
                OnLogRecieved("Aserver. News Subscribe method error: " + ex.ToString(), LogMessageType.Error);
            }
            return false;
        }
    }

    /// <summary>
    /// the news has come out
    /// </summary>
    public event Action<News> NewsEvent;

    /// <summary>
    /// new candles event
    /// </summary>
    public event Action<CandleSeries> NewCandleIncomeEvent;

    #endregion

    #region Data upload

    /// <summary>
    /// blocker of data request methods from multithreaded access
    /// </summary>
    private object _loadDataLocker = new();

    /// <summary>
    /// interface for getting the last candlesticks for a security. 
    /// Used to activate candlestick series in live trades
    /// </summary>
    public List<Candle> GetLastCandleHistory(Security security, TimeFrameBuilder timeFrameBuilder)
    {
        try
        {
            if (ServerStatus != ServerConnectStatus.Connect)
            {
                return null;
            }

            if (ServerRealization == null)
            {
                return null;
            }

            int candleCount = _keepCandlesNumber.Value;
            if (candleCount < 50)
            {
                candleCount = 50;
            }


            return ServerRealization.GetLastCandleHistory(security, timeFrameBuilder, candleCount);
        }
        catch (Exception ex)
        {
            OnLogRecieved(
                    "AServer. GetLastCandleHistory method error: " + ex.ToString(),
                    LogMessageType.Error);

            return null;
        }
    }

    /// <summary>
    /// take the candle history for a period
    /// </summary>
    // TODO: Move to DataLoader
    // Remove actualTime and needToUpdete
    // Both GetCandleDataToSecurity and GetTickDataToSecurity
    // almost completly share the same code
    public List<Candle> GetCandleDataToSecurity(string securityName, string securityClass, TimeFrameBuilder timeFrameBuilder,
            DateTime startTime, DateTime endTime, DateTime actualTime, bool needToUpdate)
    {
        try
        {
            if (Securities == null)
            {
                return null;
            }

            if (LastStartServerTime != DateTime.MinValue &&
                    LastStartServerTime.AddSeconds(5) > DateTime.Now)
            {
                return null;
            }

            if (ServerStatus != ServerConnectStatus.Connect)
            {
                return null;
            }

            Security security = null;

            for (int i = 0; Securities != null && i < Securities.Count; i++)
            {
                if (Securities[i].Name == securityName &&
                        Securities[i].NameClass == securityClass)
                {
                    security = Securities[i];
                    break;
                }
            }

            for (int i = 0; Securities != null && i < Securities.Count; i++)
            {
                if (Securities[i].NameClass == securityClass &&
                        string.IsNullOrEmpty(Securities[i].NameId) == false &&
                        Securities[i].NameId == securityName)
                {
                    security = Securities[i];
                    break;
                }
            }

            if (security == null)
            {
                for (int i = 0; Securities != null && i < Securities.Count; i++)
                {
                    if (string.IsNullOrEmpty(Securities[i].NameId) == false &&
                            Securities[i].NameId == securityName)
                    {
                        security = Securities[i];
                        break;
                    }
                }
            }

            if (security == null)
            {
                return null;
            }

            List<Candle> candles = null;

            if (timeFrameBuilder.CandleCreateMethodType == "Simple")
            {
                lock (_loadDataLocker)
                {
                    candles =
                        ServerRealization.GetCandleDataToSecurity(security, timeFrameBuilder, startTime, endTime,
                                actualTime);
                }
            }

            return candles;

        }
        catch (Exception ex)
        {
            OnLogRecieved(
                    "AServer. GetCandleDataToSecurity method error: " + ex.ToString(),
                    LogMessageType.Error);

            return null;
        }
    }

    /// <summary>
    /// take ticks data for a period
    /// </summary>
    // TODO: Move to DataLoader
    // Remove actualTime and needToUpdete
    // Both GetCandleDataToSecurity and GetTickDataToSecurity
    // almost completly share the same code
    public List<Trade> GetTickDataToSecurity(string securityName, string securityClass, DateTime startTime, DateTime endTime, DateTime actualTime, bool needToUpdete)
    {
        try
        {
            if (Securities == null)
            {
                return null;
            }

            if (LastStartServerTime != DateTime.MinValue &&
                    LastStartServerTime.AddSeconds(5) > DateTime.Now)
            {
                return null;
            }

            if (actualTime == DateTime.MinValue)
            {
                actualTime = startTime;
            }

            if (ServerStatus != ServerConnectStatus.Connect)
            {
                return null;
            }

            Security security = null;

            for (int i = 0; Securities != null && i < Securities.Count; i++)
            {
                if (Securities[i].Name == securityName &&
                        Securities[i].NameClass == securityClass)
                {
                    security = Securities[i];
                    break;
                }
            }

            if (security == null)
            {
                for (int i = 0; Securities != null && i < Securities.Count; i++)
                {
                    if (string.IsNullOrEmpty(Securities[i].NameId) == false &&
                            Securities[i].NameId == securityName)
                    {
                        security = Securities[i];
                        break;
                    }
                }
                if (security == null)
                {
                    return null;
                }
            }

            List<Trade> trades = null;

            lock (_loadDataLocker)
            {
                trades = ServerRealization.GetTickDataToSecurity(security, startTime, endTime, actualTime);
            }
            return trades;
        }
        catch (Exception ex)
        {
            OnLogRecieved(
                    "AServer. GetTickDataToSecurity method error: " + ex.ToString(),
                    LogMessageType.Error);

            return null;
        }
    }

    #endregion

    #region Trades

    /// <summary>
    /// object for accessing trades storage in the file system
    /// </summary>
    private ServerTradesStorage _tradesStorage;

    /// <summary>
    /// all server trades
    /// </summary>
    public List<Trade>[] AllTrades { get; private set; }

    /// <summary>
    /// array blocker with trades against multithreaded access
    /// </summary>
    private object _newTradesLocker = new();

    /// <summary>
    /// get trade history by security
    /// </summary>
    public List<Trade> GetAllTradesToSecurity(Security security)
    {
        try
        {
            if (AllTrades == null)
            {
                return null;
            }
            List<Trade> trades = [];

            for (int i = 0; i < AllTrades.Length; i++)
            {
                if (AllTrades[i] != null && AllTrades[i].Count != 0 &&
                        AllTrades[i][0] != null &&
                        AllTrades[i][0].SecurityNameCode == security.Name)
                {
                    return AllTrades[i];
                }
            }

            return trades;
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
            return null;
        }
    }

    /// <summary>
    /// upload trades by market depth data
    /// </summary>
    private void BathTradeMarketDepthData(Trade trade)
    {
        MarketDepth depth = null;

        lock (_depthsArrayLocker)
        {
            for (int i = 0; i < _depths.Count; i++)
            {
                if (_depths[i].SecurityNameCode == trade.SecurityNameCode)
                {
                    depth = _depths[i];
                    break;
                }
            }
        }

        if (depth == null) { return; }

        if (depth.Asks != null &&
                depth.Asks.Count > 0)
        {
            trade.Ask = depth.Asks[0].Price;
        }

        if (depth.Bids != null &&
                depth.Bids.Count > 0)
        {
            trade.Bid = depth.Bids[0].Price;
        }

        trade.BidsVolume = depth.Bids.Sum(b => b.Bid);
        trade.AsksVolume = depth.Asks.Sum(a => a.Ask);
    }

    /// <summary>
    /// new trade event
    /// </summary>
    public event Action<List<Trade>> NewTradeEvent;

    #endregion

    #region MyTrade

    /// <summary>
    /// my trades array
    /// </summary>
    // public List<MyTrade> MyTrades
    // {
    //     get { return _myTrades; }
    // }
    // private List<MyTrade> _myTrades = [];
    public Queue<MyTrade> MyTrades => _userTrades;
    private Queue<MyTrade> _userTrades = new(1000);

    /// <summary>
    /// whether a sound must be emitted during a new my trade
    /// </summary>
    private bool _needToBeepOnTrade;

    /// <summary>
    /// buzzer mechanism 
    /// </summary>
    private async void MyTradesBeepThread()
    {
        while (MainWindow.ProccesIsWorked)
        {
            await Task.Delay(2000);

            if (PrimeSettingsMaster.TransactionBeepIsActive
                    && _needToBeepOnTrade)
            {
                _needToBeepOnTrade = false;
                SystemSounds.Asterisk.Play();
            }
        }
    }

    /// <summary>
    /// my trade changed event
    /// </summary>
    public event Action<MyTrade> NewMyTradeEvent;

    #endregion

    #region Compare positions module

    // FIX:
    // public ComparePositionsModule ComparePositionsModule;
    //
    // public void ShowComparePositionsModuleDialog(string portfolioName)
    // {
    //     ComparePositionsModuleUi myUi = null;
    //
    //     for (int i = 0; i < _comparePositionsModuleUi.Count; i++)
    //     {
    //         if (_comparePositionsModuleUi[i].PortfolioName == portfolioName)
    //         {
    //             myUi = _comparePositionsModuleUi[i];
    //             break;
    //         }
    //     }
    //
    //     if (myUi == null)
    //     {
    //         myUi = new ComparePositionsModuleUi(ComparePositionsModule, portfolioName);
    //         myUi.GuiClosed += MyUi_GuiClosed;
    //         _comparePositionsModuleUi.Add(myUi);
    //         myUi.Show();
    //     }
    //     else
    //     {
    //         myUi.Activate();
    //     }
    // }
    //
    // private void MyUi_GuiClosed(string portfolioName)
    // {
    //     for (int i = 0; i < _comparePositionsModuleUi.Count; i++)
    //     {
    //         if (_comparePositionsModuleUi[i].PortfolioName == portfolioName)
    //         {
    //             _comparePositionsModuleUi[i].GuiClosed -= MyUi_GuiClosed;
    //             _comparePositionsModuleUi.RemoveAt(i);
    //             break;
    //         }
    //     }
    // }
    //
    // private List<ComparePositionsModuleUi> _comparePositionsModuleUi = new List<ComparePositionsModuleUi>();

    #endregion


    #region Additional Market Data

    private Dictionary<string, OptionMarketData> _dictAdditionalMarketData = [];

    private void ConvertableMarketData(OptionMarketDataForConnector data)
    {
        try
        {
            if (string.IsNullOrEmpty(data.SecurityName))
            {
                return;
            }

            if (!_dictAdditionalMarketData.TryGetValue(data.SecurityName, out OptionMarketData value))
            {
                value = new();
                _dictAdditionalMarketData.Add(data.SecurityName, value);
            }

            UpdateValueIfChanged(ref value.SecurityName, data.SecurityName);
            UpdateValueIfChanged(ref value.UnderlyingAsset, data.UnderlyingAsset);
            UpdateValueIfChanged(ref value.UnderlyingPrice, data.UnderlyingPrice);
            UpdateValueIfChanged(ref value.MarkPrice, data.MarkPrice);
            UpdateValueIfChanged(ref value.MarkIV, data.MarkIV);
            UpdateValueIfChanged(ref value.BidIV, data.BidIV);
            UpdateValueIfChanged(ref value.AskIV, data.AskIV);
            UpdateValueIfChanged(ref value.Delta, data.Delta);
            UpdateValueIfChanged(ref value.Gamma, data.Gamma);
            UpdateValueIfChanged(ref value.Vega, data.Vega);
            UpdateValueIfChanged(ref value.Theta, data.Theta);
            UpdateValueIfChanged(ref value.Rho, data.Rho);
            UpdateValueIfChanged(ref value.OpenInterest, data.OpenInterest);

            if (!string.IsNullOrEmpty(data.TimeCreate) &&
                    value.TimeCreate.ToString() != data.TimeCreate)
            {
                value.TimeCreate = TimeManager.GetDateTimeFromTimeStamp(Convert.ToInt64(data.TimeCreate));
            }

            NewAdditionalMarketDataEvent?.Invoke(value);
        }
        catch (Exception ex)
        {
            OnLogRecieved(ex.ToString(), LogMessageType.Error);
            Thread.Sleep(5000);
        }
    }

    private static void UpdateValueIfChanged<T>(ref T target, string newValue)
    {
        if (!string.IsNullOrEmpty(newValue) && target.ToString() != newValue)
        {
            target = (T)Convert.ChangeType(newValue, typeof(T));
        }
    }

    #region Log messages

    /// <summary>
    /// log manager
    /// </summary>
    public Logs Logs;

    /// <summary>
    /// add a new message in the log
    /// </summary>
    private void SendLogMessage(string message, LogMessageType type)
    {
        if (Permissions.SupportsMultipleServers)
        {
            message = ServerNameUnique + " " + message;
        }

        LogRecieved?.Invoke(message, type);
    }

    public void OnLogRecieved(string message, LogMessageType type)
    {
        if (Permissions.SupportsMultipleServers)
        {
            message = ServerNameUnique + " " + message;
        }

        LogRecieved?.Invoke(message, type);
    }

    /// <summary>
    /// outgoing messages for the log event
    /// </summary>
    public event Action<string, LogMessageType> LogMessageEvent
    {
        add => LogRecieved += LogRecieved;
        remove => LogRecieved += LogRecieved;
    }
    public event Action<string, LogMessageType> LogRecieved;

    #endregion

    /// <summary>
    /// new Additional Market Data
    /// </summary>
    public event Action<OptionMarketData> NewAdditionalMarketDataEvent;
    public event Action<Funding> NewFundingEvent;
    public event Action<SecurityVolumes> NewVolume24hUpdateEvent;

    #endregion
}

class SecurityFlowTime
{
    public string SecurityName;

    public string SecurityClass;

    public DateTime LastTimeTrade;

    public DateTime LastTimeMarketDepth;
}
