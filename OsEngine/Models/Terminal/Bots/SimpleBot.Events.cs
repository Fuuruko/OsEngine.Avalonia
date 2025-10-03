using System;
using System.Collections.Generic;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;

namespace OsEngine.Models.Terminal.Bots;

public partial class SimpleBot
{
    /// <summary>
    /// My new trade event
    /// </summary>
    [Obsolete($"Use {nameof(UserTradeEvent)} instead")]
    public event Action<MyTrade> MyTradeEvent;
    public event Action<MyTrade> UserTradeEvent;

    /// <summary>
    /// Updated order
    /// </summary>
    public event Action<Order> OrderUpdated;
    [Obsolete($"Use {nameof(OrderUpdated)} instead")]
    public event Action<Order> OrderUpdateEvent;

    /// <summary>
    /// An attempt to revoke the order ended in an error
    /// </summary>
    public event Action<Order> CancelOrderFailEvent;

    /// <summary>
    /// New trades
    /// </summary>
    public event Action<Trade> NewTickEvent;

    /// <summary>
    /// New server time
    /// </summary>
    public event Action<DateTime> ServerTimeChanged;
    [Obsolete($"Use {nameof(ServerTimeChanged)} instead")]
    public event Action<DateTime> ServerTimeChangeEvent;

    /// <summary>
    /// Last candle finished
    /// </summary>
    [Obsolete($"Use {nameof(CandleFinished)} instead")]
    public event Action<List<Candle>> CandleFinishedEvent;
    public event Action<List<Candle>> CandleFinished;

    /// <summary>
    /// Last candle update
    /// </summary>
    [Obsolete($"Use {nameof(CandleUpdated)} instead")]
    public event Action<List<Candle>> CandleUpdateEvent;
    public event Action<List<Candle>> CandleUpdated;

    /// <summary>
    /// New marketDepth
    /// </summary>
    [Obsolete($"Use {nameof(MarketDepthUpdated)} instead")]
    public event Action<MarketDepth> MarketDepthUpdateEvent;
    public event Action<MarketDepth> MarketDepthUpdated;

    /// <summary>
    /// Bid ask change
    /// </summary>
    [Obsolete($"Use {nameof(BestBidAskChanged)} instead")]
    public event Action<decimal, decimal> BestBidAskChangeEvent;
    public event Action<decimal, decimal> BestBidAskChanged;

    /// <summary>
    /// Position successfully closed
    /// </summary>
    [Obsolete($"Use {nameof(PositionClosingSuccessed)} instead")]
    public event Action<Position> PositionClosingSuccesEvent;
    public event Action<Position> PositionClosingSuccessed;

    /// <summary>
    /// Position successfully opened
    /// </summary>
    [Obsolete($"Use {nameof(PositionOpeningSuccessed)} instead")]
    public event Action<Position> PositionOpeningSuccesEvent;
    public event Action<Position> PositionOpeningSuccessed;

    /// <summary>
    /// Open position volume has changed
    /// </summary>
    [Obsolete($"Use {nameof(PositionNetVolumeChanged)} instead")]
    public event Action<Position> PositionNetVolumeChangeEvent;
    public event Action<Position> PositionNetVolumeChanged;

    /// <summary>
    /// Opening position failed
    /// </summary>
    [Obsolete($"Use {nameof(PositionOpeningFailed)} instead")]
    public event Action<Position> PositionOpeningFailEvent;
    public event Action<Position> PositionOpeningFailed;

    /// <summary>
    /// Position closing failed
    /// </summary>
    [Obsolete($"Use {nameof(PositionClosingFailed)} instead")]
    public event Action<Position> PositionClosingFailEvent;
    public event Action<Position> PositionClosingFailed;

    /// <summary>
    /// A stop order is activated for the position
    /// </summary>
    [Obsolete($"Use {nameof(PositionStopActivated)} instead")]
    public event Action<Position> PositionStopActivateEvent;
    public event Action<Position> PositionStopActivated;

    /// <summary>
    /// A profit order is activated for the position
    /// </summary>
    [Obsolete($"Use {nameof(PositionProfitActivated)} instead")]
    public event Action<Position> PositionProfitActivateEvent;
    public event Action<Position> PositionProfitActivated;


    // NOTE: Rename?
    public event Action<Position> PositionOpenAtStopActivated;

    /// <summary>
    /// Stop order buy activated
    /// </summary>
    [Obsolete($"Use {nameof(PositionOpenAtStopActivated)} instead")]
    public event Action<Position> PositionBuyAtStopActivateEvent;

    /// <summary>
    /// Stop order sell activated
    /// </summary>
    [Obsolete($"Use {nameof(PositionOpenAtStopActivated)} instead")]
    public event Action<Position> PositionSellAtStopActivateEvent;

    /// <summary>
    /// Portfolio on exchange changed
    /// </summary>
    [Obsolete($"Use {nameof(PortfolioOnExchangeChanged)} instead")]
    public event Action<Portfolio> PortfolioOnExchangeChangedEvent;
    public event Action<Portfolio> PortfolioOnExchangeChanged;

    /// <summary>
    /// The morning session started. Send the first trades
    /// </summary>
    // [Obsolete($"Use {nameof(FirstTickToDay)} instead")]
    public event Action<Trade> FirstTickToDayEvent;
    // public event Action<Trade> FirstTickToDay;

    /// <summary>
    /// Indicator parameters changed
    /// </summary>
    // [Obsolete($"Use {nameof(IndicatorUpdate)} instead")]
    // public event Action IndicatorUpdateEvent;
    // public event Action IndicatorUpdate;

    /// <summary>
    /// Security for connector defined
    /// </summary>
    [Obsolete($"Use {nameof(SecuritySubscribe)} instead")]
    public event Action<Security> SecuritySubscribeEvent;
    public event Action<Security> SecuritySubscribe;

    /// <summary>
    /// Source removed
    /// </summary>
    // public event Action TabDeletedEvent;

    /// <summary>
    /// The robot is removed from the system
    /// </summary>
    // public event Action<int> DeleteBotEvent;
    //
    // public event Action<IIndicator, BotTabSimple> IndicatorManuallyCreateEvent;
    //
    // public event Action<IIndicator, BotTabSimple> IndicatorManuallyDeleteEvent;
}
