using System;
using System.Collections.Generic;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;
using OsEngine.Models.Market.Connectors;
using OsEngine.Models.Market.Servers;

namespace OsEngine.Models.Terminal.Bots;

public partial class SimpleBot
{
    private bool _isDelete;

    /// <summary>
    /// has the session started today?
    /// </summary>
    private bool _firstTickToDaySend;

    private DateTime _lastTradeTime;

    private decimal _lastTradeQty;

    private decimal _lastTradePrice;

    private int _lastTradeIndex;

    private long _lastTradeIdInTester;

    [Obsolete($"Use {nameof(Connector)} instead")]
    private ConnectorCandles _connector
    {
        get => Connector;
        set => Connector = value;
    }
    public ConnectorCandles Connector { get; private set; }
    public decimal PriceBestBid;
    public decimal PriceBestAsk;

    [Obsolete($"Use {nameof(ServerTime)} instead")]
    public DateTime TimeServerCurrent => ServerTime;
    public DateTime ServerTime
    {
        get
        {
            if (Connector == null)
            {
                return DateTime.MinValue;
            }
            return Connector.MarketTime;
        }
    }

    /// <summary>
    ///  The status of the server to which the tab is connected
    /// </summary>
    public ServerConnectStatus ServerStatus
    {
        get
        {
            if (StartProgram == StartProgram.IsOsOptimizer)
            {
                return ServerConnectStatus.Connect;
            }

            if (Connector == null)
            {
                return ServerConnectStatus.Disconnect;
            }

            IServer myServer = Connector.MyServer;

            if (myServer == null)
            {
                return ServerConnectStatus.Disconnect;
            }

            return myServer.ServerStatus;
        }
    }

    /// <summary>
    /// candle is update
    /// </summary>
    private void LogicToUpdateLastCandle(List<Candle> candles)
    {
        try
        {
            if (_isDelete)
            {
                return;
            }
            LastTimeCandleUpdate = Connector.MarketTime;

            // FIX:
            // AlertControlPosition();

            // while (_chartMaster == null)
            // {
            //     Task delay = new Task(() =>
            //             {
            //             Thread.Sleep(100);
            //             });
            //
            //     delay.Start();
            //     delay.Wait();
            // }

            // _chartMaster.SetCandles(candles);
            CandleUpdateEvent?.Invoke(candles);

        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// candle is finished
    /// </summary>
    /// <param name="candles">candles</param>
    private void LogicToEndCandle(List<Candle> candles)
    {
        try
        {
            if (_isDelete)
            {
                return;
            }
            if (candles == null)
            {
                return;
            }
            // FIX:
            // AlertControlPosition();

            if (PositionOpenerToStop != null &&
                    PositionOpenerToStop.Count != 0)
            {
                CancelStopOpenerByNewCandle(candles);
            }

            // if (_chartMaster != null)
            // {
            //     _chartMaster.SetCandles(candles);
            // }

            try
            {
                CandleFinishedEvent?.Invoke(candles);
            }
            catch (Exception error)
            {
                OnLogRecieved(error.ToString(), LogMessageType.Error);
            }
        }
        catch (Exception error)
        {
            OnLogRecieved(error.ToString(), LogMessageType.Error);
        }
    }

    /// <summary>
    /// new tiki came
    /// </summary>
    private void _connector_TickChangeEvent(List<Trade> trades)
    {
        if (_isDelete)
        {
            return;
        }

        if (trades == null ||
                trades.Count == 0 ||
                trades[^1] == null)
        {
            return;
        }

        // if (_chartMaster == null)
        // {
        //     return;
        // }

        if ((StartProgram == StartProgram.IsOsOptimizer
                    || StartProgram == StartProgram.IsTester)
                && trades.Count < 10)
        {
            _lastTradeTime = DateTime.MinValue;
            _lastTradeIndex = 0;
            _lastTradeIdInTester = 0;
            return;
        }

        if (StartProgram == StartProgram.IsOsTrader)
        {
            if (ServerStatus == ServerConnectStatus.Disconnect)
            {
                return;
            }

            if (_lastTradeTime == DateTime.MinValue &&
                    _lastTradeIndex == 0)
            {
                _lastTradeIndex = trades.Count;
                _lastTradeTime = trades[^1].Time;
                return;
            }
        }
        else if (StartProgram == StartProgram.IsTester ||
                StartProgram == StartProgram.IsOsOptimizer)
        {
            if (trades[^1].TimeFrameInTester != TimeFrame.Sec1 &&
                    trades[^1].TimeFrameInTester != Connector.TimeFrame)
            {
                return;
            }
        }

        Trade trade = trades[^1];

        if (trade != null && _firstTickToDaySend == false && FirstTickToDayEvent != null)
        {
            if (trade.Time.Hour == 10
                    && (trade.Time.Minute == 1 || trade.Time.Minute == 0))
            {
                _firstTickToDaySend = true;
                FirstTickToDayEvent(trade);
            }
        }

        List<Trade> newTrades = [];

        if (StartProgram == StartProgram.IsOsTrader)
        {
            if (trades.Count > 1000)
            { // if deleting trades from the system is disabled

                int newTradesCount = trades.Count - _lastTradeIndex;

                if (newTradesCount <= 0)
                {
                    return;
                }

                newTrades = trades.GetRange(_lastTradeIndex, newTradesCount);
            }
            else
            {
                if (_lastTradeTime == DateTime.MinValue)
                {
                    newTrades = trades;
                }
                else
                {
                    for (int i = 0; i < trades.Count; i++)
                    {
                        try
                        {
                            if (trades[i] == null)
                            {
                                continue;
                            }

                            if (trades[i].Time < _lastTradeTime)
                            {
                                continue;
                            }
                            if (trades[i].Time == _lastTradeTime
                                    && trades[i].Price == _lastTradePrice
                                    && trades[i].Volume == _lastTradeQty)
                            {
                                continue;
                            }
                            newTrades.Add(trades[i]);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }
        else // Tester, Optimizer
        {
            if (_lastTradeTime == DateTime.MinValue)
            {
                newTrades = trades;
                _lastTradeIdInTester = newTrades[^1].IdInTester;
            }
            else
            {
                for (int i = trades.Count - 1; i < trades.Count; i--)
                {
                    try
                    {
                        if (trades[i].IdInTester <= _lastTradeIdInTester)
                        {
                            break;
                        }

                        newTrades.Insert(0, trades[i]);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        if (newTrades.Count == 0)
        {
            return;
        }

        for (int i2 = 0; i2 < newTrades.Count; i2++)
        {
            if (newTrades[i2] == null)
            {
                newTrades.RemoveAt(i2);
                i2--;
                continue;
            }
        }

        if (Journal == null)
        {
            return;
        }

        if (_isDelete)
        {
            return;
        }

        IReadOnlyList<Position> openPositions = Journal.OpenPositions;

        if (openPositions != null)
        {
            for (int i = 0; i < openPositions.Count; i++)
            {
                if (openPositions[i].StopOrderIsActive == false &&
                        openPositions[i].ProfitOrderIsActive == false)
                {
                    continue;
                }

                for (int i2 = 0; i < openPositions.Count && i2 < newTrades.Count; i2++)
                {
                    if (CheckStop(openPositions[i], newTrades[i2].Price))
                    {
                        if (StartProgram != StartProgram.IsOsTrader)
                        {
                            i--;
                        }
                        break;
                    }
                }
            }
        }

        if (PositionOpenerToStop != null &&
                PositionOpenerToStop.Count != 0)
        {
            for (int i2 = 0; i2 < newTrades.Count; i2++)
            {
                CheckStopOpener(newTrades[i2].Price);
            }
        }
        if (NewTickEvent != null)
        {
            for (int i2 = 0; i2 < newTrades.Count; i2++)
            {
                try
                {
                    NewTickEvent(newTrades[i2]);
                }
                catch (Exception error)
                {
                    OnLogRecieved(error.ToString(), LogMessageType.Error);
                }
            }
        }

        if (Connector.EmulatorIsOn == true)
        {
            for (int i2 = 0; i2 < newTrades.Count; i2++)
            {
                try
                {
                    Connector.CheckEmulatorExecution(newTrades[i2].Price);
                }
                catch (Exception error)
                {
                    OnLogRecieved(error.ToString(), LogMessageType.Error);
                }
            }
        }

        _lastTradeIndex = trades.Count;
        _lastTradeTime = newTrades[^1].Time;
        _lastTradeIdInTester = newTrades[^1].IdInTester;
        _lastTradeQty = newTrades[^1].Volume;
        _lastTradePrice = newTrades[^1].Price;

        if (StartProgram == StartProgram.IsOsTrader)
        {
            CheckSurplusPositions();
        }
    }

    /// <summary>
    /// Incoming my deal
    /// </summary>
    private void _connector_MyTradeEvent(MyTrade trade)
    {
        if (_isDelete)
        {
            return;
        }
        if (Journal.SetNewMyTrade(trade) == false)
        {
            return;
        }

        MyTradeEvent?.Invoke(trade);
    }

    /// <summary>
    /// Security for connector defined
    /// </summary>
    private void _connector_SecuritySubscribeEvent(Security security)
    {
        SecuritySubscribeEvent?.Invoke(security);
    }

    /// <summary>
    /// Server time has changed
    /// </summary>
    void StrategOneSecurity_TimeServerChangeEvent(DateTime time)
    {
        if (_isDelete)
        {
            return;
        }
        if (ManualPositionSupport != null)
        {
            ManualPositionSupport.ServerTime = time;
        }

        ServerTimeChangeEvent?.Invoke(time);
    }

    /// <summary>
    /// Incoming orders
    /// </summary>
    private void _connector_OrderChangeEvent(Order order)
    {
        if (_isDelete)
        {
            return;
        }
        Order orderInJournal = Journal.IsMyOrder(order);

        if (orderInJournal == null)
        {
            return;
        }
        Journal.SetUpdateOrderInPositions(order);
        // FIX:
        // _icebergMaker.SetNewOrder(order);

        OrderUpdateEvent?.Invoke(orderInJournal);
        // _chartMaster.SetPosition(PositionsAll);
    }

    /// <summary>
    /// An attempt to revoke the order ended in an error
    /// </summary>
    private void _connector_CancelOrderFailEvent(Order order)
    {
        if (_isDelete)
        {
            return;
        }

        Order orderInJournal = Journal.IsMyOrder(order);

        if (orderInJournal == null)
        {
            return;
        }

        CancelOrderFailEvent?.Invoke(orderInJournal);
    }

    /// <summary>
    /// Incoming new bid with ask
    /// </summary>
    private void _connector_BestBidAskChangeEvent(decimal bestBid, decimal bestAsk)
    {
        if (_isDelete)
        {
            return;
        }
        Journal?.SetBidAsk(bestBid, bestAsk);
        // FIX:
        // _marketDepthPainter?.ProcessBidAsk(bestBid, bestAsk);
        BestBidAskChangeEvent?.Invoke(bestBid, bestAsk);
    }

    private void _connector_NewVolume24hChangedEvent(SecurityVolumes data)
    {
        SecurityVolumes.SecurityNameCode = data.SecurityNameCode;
        SecurityVolumes.Volume24h = data.Volume24h;
        SecurityVolumes.Volume24hUSDT = data.Volume24hUSDT;
        SecurityVolumes.TimeUpdate = data.TimeUpdate;
    }

    private void _connector_FundingChangedEvent(Funding data)
    {
        Funding.SecurityNameCode = data.SecurityNameCode;
        Funding.CurrentValue = data.CurrentValue;
        Funding.NextFundingTime = data.NextFundingTime;
        Funding.FundingIntervalHours = data.FundingIntervalHours;
        Funding.MaxFundingRate = data.MaxFundingRate;
        Funding.MinFundingRate = data.MinFundingRate;
        Funding.TimeUpdate = data.TimeUpdate;
    }

    private void _connector_ConnectorStartedReconnectEvent(string securityName, TimeFrame timeFrame, TimeSpan timeFrameSpan, string portfolioName, string serverType)
    {
        _lastTradeTime = DateTime.MinValue;
        _lastTradeIndex = 0;
        _lastTradeIdInTester = 0;

        // if (_chartMaster == null)
        // {
        //     return;
        // }
        // FIX:
        // _chartMaster.ClearTimePoints();

        if (string.IsNullOrEmpty(securityName)) { return; }

        // FIX:
        // _chartMaster.SetNewSecurity(securityName, _connector.TimeFrameBuilder, portfolioName, serverType);
    }

    private void _connector_GlassChangeEvent(MarketDepth marketDepth)
    {
        if (_isDelete)
        {
            return;
        }
        MarketDepth = marketDepth;

        // if (_marketDepthPainter != null)
        // {
        //     _marketDepthPainter.ProcessMarketDepth(marketDepth);
        // }

        MarketDepthUpdateEvent?.Invoke(marketDepth);

        if (StartProgram == StartProgram.IsOsTrader) { return; }

        if ((marketDepth.Asks == null || marketDepth.Asks.Count == 0)
                &&
                (marketDepth.Bids == null || marketDepth.Bids.Count == 0))
        {
            return;
        }

        var openPositions = Journal.OpenPositions;

        if (openPositions == null) { return; }

        for (int i = 0; i < openPositions.Count; i++)
        {
            if (openPositions[i].State != PositionStateType.Open)
            {
                continue;
            }

            if (marketDepth.Asks != null && marketDepth.Asks.Count > 0)
            {
                CheckStop(openPositions[i], marketDepth.Asks[0].Price);
            }

            if (openPositions.Count <= i)
            {
                continue;
            }

            if (marketDepth.Bids != null && marketDepth.Bids.Count > 0)
            {
                CheckStop(openPositions[i], marketDepth.Bids[0].Price);
            }
        }
    }
}
