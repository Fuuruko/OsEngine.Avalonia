using System;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Market.Servers.Tester;

/// <summary>
/// Data storage type
/// </summary>
public enum TesterSourceDataType
{
    Set,

    Folder
}

/// <summary>
/// Current operation mode of the tester
/// </summary>
public enum TesterRegime
{
    NotActive,

    Pause,

    Play,

    PlusOne,
}

/// <summary>
/// Type of data translation from the tester
/// </summary>
[Flags]
public enum TesterDataType
{
    Unknown = 0,
    None = Unknown,
    Candle = 1,
    [Name("Ticks: All States")]
    TickAllCandleState = 2,
    // NOTE: What difference between candle this?
    [Name("Ticks: Only Ready Candle")]
    TickOnlyReadyCandle = 4,
    [Name("DOM: All States")]
    MarketDepthAllCandleState = 8,
    // DOM_AllCandleState = MarketDepthAllCandleState,
    // NOTE: What difference between candle this?
    [Name("DOM: Only Ready Candle")]
    MarketDepthOnlyReadyCandle = 16,
    [Name("Ticks: Cumulative")]
    // [Description("Cumulative Ticks will will combine Ticks " +
                 // "with same Price and Direction to one Tick")]
    CumulativeTicks = 32,
    Tick = TickAllCandleState | TickOnlyReadyCandle,
    MarketDepth = MarketDepthAllCandleState | MarketDepthOnlyReadyCandle,
    OnlyReadyCandles = TickOnlyReadyCandle | MarketDepthOnlyReadyCandle,
}

/// <summary>
/// Type of data stored
/// </summary>
public enum SecurityTesterDataType
{
    Candle,
    Tick,
    MarketDepth
}

/// <summary>
/// Time step in the synchronizer
/// </summary>
[Obsolete]
public enum TimeAddInTestType
{
    Millisecond,
    Second,
    Minute,
    FiveMinute,
}

/// <summary>
/// type of limit order execution
/// </summary>
public enum OrderExecutionType
{
    Touch,
    Intersection,
}

