using System.Collections.Generic;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Market.Servers;

public partial class BaseServer
{
    public List<IBaseInput> Inputs { get; private set; } = new(12);

    private Input.String _namePostfix = new("Name Postfix", "");

    /// <summary>
    /// whether to save the current session's trades to the file system
    /// </summary>
    // NOTE: Can be moved to tickStorage
    private Input.Bool _isKeepTrades =
        new("Keep trade history", false);

    /// <summary>
    /// parameter with the number of days for saving ticks
    /// </summary>
    // NOTE: Can be moved to tickStorage
    private Input.Int _uploadTradesDaysNumber =
        new("Days to load trades", 5);

    /// <summary>
    /// whether candles should be saved to the file system
    /// </summary>
    // NOTE: Can be moved to CandleStorage (and CandleManager?)
    private Input.Bool _isKeepCandles =
        new("Keep candle history", true);

    /// <summary>
    /// number of candles for which trades should be loaded at the start of the connector
    /// </summary>
    // NOTE: Can be moved to CandleStorage (and CandleManager?)
    // NOTE: Should be done automatically probably?
    private Input.Int _keepCandlesNumber =
        new("Candles to load", 300);

    /// <summary>
    /// whether trades should be filled with data on the best bid and ask.
    /// </summary>
    private Input.Bool _needToLoadBidAskInTrades2 =
        new("Bid Ask in trades", false);

    /// <summary>
    /// whether to delete the transaction feed from memory
    /// </summary>
    // [Description(
    //         "If true - arrays with deals will be " +
    //         "automatically cleared inside the program.")]
    private Input.Bool _isClearTrades =
        new("Delete trades from memory", true);
        // {
        //     ToolTip =
        //         "If true - arrays with deals will be " +
        //         "automatically cleared inside the program."
        // };

    /// <summary>
    /// whether the candles should be removed from the memory
    /// </summary>
    // NOTE: Can be moved to CandleManager
    private Input.Bool _isClearCandles =
        new("Remove Candles From Memory", false);

    /// <summary>
    /// whether we use the full stack of market depth or only bid and ask.
    /// </summary>
    private Input.Bool _needToUseFullMarketDepth2 =
        new("Use Full Market Depth", true);

    /// <summary>
    /// only trades with a new price are submitted to the top.
    /// </summary>
    private Input.Bool _isUpdateOnlyNewPriceTrades =
        new("Skip trades with the same price", true);
}
