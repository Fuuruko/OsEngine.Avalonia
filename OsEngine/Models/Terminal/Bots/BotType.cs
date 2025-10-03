namespace OsEngine.Models.Terminal;

public enum BotTabType
{
    /// <summary>
    /// source for trading one security
    /// </summary>
    Simple,

    /// <summary>
    /// source for index creation
    /// </summary>
    Index,

    /// <summary>
    /// source for creating and displaying a cluster chart
    /// </summary>
    Cluster,

    /// <summary>
    /// source for trading multiple securities
    /// </summary>
    Screener,

    /// <summary>
    ///  source for trading pairs
    /// </summary>
    Pair,

    /// <summary>
    /// source for trading currency arbitrage
    /// </summary>
    Polygon,
    Arbitrage = Polygon,

    /// <summary>
    ///  source for the news feed
    /// </summary>
    News
}
