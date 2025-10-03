using System;
using System.Collections.Generic;
using System.Linq;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Logging;
using OsEngine.Models.Market.Connectors;
using OsEngine.Models.Market.Servers;
using OsEngine.Models.Terminal.Controls;

namespace OsEngine.Models.Terminal.Bots;

public partial class SimpleBot : BaseBot, IBot
{

    public Journal Journal;
    public Security Security { get; set; }
    public Portfolio Portfolio { get; set; }

    public List<Position> Positions { get; set; }
    public List<Position> PositionsOpenAll { get; set; }

    public Commission Commission { get; } = new()
    {
        Value = 0,
        Type = CommissionType.None,
    };

    internal BotManualControl ManualPositionSupport;

    public bool IsConnected;
    public bool IsReadyToTrade;

    public BotTabType TabType => BotTabType.Simple;

    /// <summary>
    /// All candles of the instrument. Only completed
    /// </summary>
    // NOTE: create new list every time called. better not use
    public List<Candle> CandlesFinishedOnly
    {
        get
        {
            if (Connector == null)
            {
                return null;
            }
            return Connector.Candles(true);
        }
    }


    // XXX: May return null or empty list
    public Candle LastFinishedCandle
    {
        get
        {
            // if (Connector.)
            return Connector.Candles(false)?[^1];
        }
    }

    public int FinishedCandlesCount => FinishedCandles.Count();
    public IEnumerable<Candle> FinishedCandles
    {
        get
        {
            List<Candle> candles = Connector.Candles(false);
            if (candles == null) { return null; }
            if (candles[^1].IsActive)
            {
                return candles;
            }
            else
            {
                return candles.Take(candles.Count - 1);
            }
        }
    }

    public MarketDepth MarketDepth { get; set; }

    /// <summary>
    /// Stop opening waiting for its price
    /// </summary>
    public List<PositionOpenerToStopLimit> PositionOpenerToStop;

    internal SharedValue<bool> IsEnabled { get; set; }
    public bool EventsIsOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public BaseServer Server;

    internal SharedValue<bool> IsEmulatorOn { get; set; }
    public bool EmulatorIsOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Funding Funding { get; } = new Funding();
    public SecurityVolumes SecurityVolumes { get; } = new SecurityVolumes();


    public SimpleBot(string name, StartProgram startProgram)
    {
        Name = name;
        StartProgram = startProgram;

        Journal = new(name, startProgram)
        {
            Commission = Commission,
        };


        Buy = new BuySellOperations(this, Side.Buy);
        Sell = new BuySellOperations(this, Side.Sell);
        Close = new CloseOperations(this);

        _connector = new ConnectorCandles(Name, startProgram, true);
        _connector.OrderChangeEvent += _connector_OrderChangeEvent;
        _connector.CancelOrderFailEvent += _connector_CancelOrderFailEvent;
        _connector.MyTradeEvent += _connector_MyTradeEvent;
        _connector.BestBidAskChangeEvent += _connector_BestBidAskChangeEvent;
        _connector.PortfolioOnExchangeChangedEvent +=
            (portfolio) => PortfolioOnExchangeChanged?.Invoke(portfolio);
        _connector.GlassChangeEvent += _connector_GlassChangeEvent;
        _connector.TimeChangeEvent += StrategOneSecurity_TimeServerChangeEvent;
        _connector.NewCandlesChangeEvent += LogicToEndCandle;
        _connector.LastCandlesChangeEvent += LogicToUpdateLastCandle;
        _connector.TickChangeEvent += _connector_TickChangeEvent;
        _connector.LogMessageEvent += SetNewLogMessage;
        _connector.ConnectorStartedReconnectEvent += _connector_ConnectorStartedReconnectEvent;
        _connector.SecuritySubscribeEvent += _connector_SecuritySubscribeEvent;
        // _connector.SecuritySubscribeEvent += 
            // (security) => SecuritySubscribe?.Invoke(security);

        // _connector.DialogClosed += _connector_DialogClosed;
        _connector.FundingChangedEvent += _connector_FundingChangedEvent;
        _connector.NewVolume24hChangedEvent += _connector_NewVolume24hChangedEvent;

        // if (startProgram != StartProgram.IsOsOptimizer)
        // {
        //     _marketDepthPainter = new MarketDepthPainter(TabName);
        //     _marketDepthPainter.LogMessageEvent += SetNewLogMessage;
        // }

        // if (startProgram == StartProgram.IsOsTrader)
        // {// load the latest orders for robots to the general storage in ServerMaster
        //
        //     List<Order> oldOrders = Journal.GetLastOrdersToPositions(50);
        //
        //     for (int i = 0; i < oldOrders.Count; i++)
        //     {
        //         ServerMaster.InsertOrder(oldOrders[i]);
        //     }
        // }

    }

    public void Delete()
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    // Outgoing events. Handlers for strategy

}
