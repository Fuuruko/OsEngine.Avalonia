/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;

namespace OsEngine.Models.Market.Connectors;

public partial class ConnectorCandles
{
    /// <summary>
    /// order are changed
    /// </summary>
    public event Action<Order> OrderChangeEvent;

    /// <summary>
    /// an attempt to revoke the order ended in an error
    /// </summary>
    public event Action<Order> CancelOrderFailEvent;

    /// <summary>
    /// another candle has closed
    /// </summary>
    public event Action<List<Candle>> NewCandlesChangeEvent;

    /// <summary>
    /// candles are changed
    /// </summary>
    public event Action<List<Candle>> LastCandlesChangeEvent;

    /// <summary>
    /// market depth is changed
    /// </summary>
    public event Action<MarketDepth> GlassChangeEvent;

    /// <summary>
    /// myTrade are changed
    /// </summary>
    public event Action<MyTrade> MyTradeEvent;

    /// <summary>
    /// new trade in the trades feed
    /// </summary>
    public event Action<List<Trade>> TickChangeEvent;

    /// <summary>
    /// bid or ask is changed
    /// </summary>
    public event Action<decimal, decimal> BestBidAskChangeEvent;

    /// <summary>
    /// testing finished
    /// </summary>
    public event Action TestOverEvent;

    /// <summary>
    /// testing started
    /// </summary>
    public event Action TestStartEvent;

    /// <summary>
    /// server time is changed
    /// </summary>
    public event Action<DateTime> TimeChangeEvent;

    /// <summary>
    /// connector is starting to reconnect
    /// </summary>
    public event Action<string, TimeFrame, TimeSpan, string, string> ConnectorStartedReconnectEvent;

    /// <summary>
    /// security for connector defined
    /// </summary>
    public event Action<Security> SecuritySubscribeEvent;

    /// <summary>
    /// portfolio on exchange changed
    /// </summary>
    public event Action<Portfolio> PortfolioOnExchangeChangedEvent;

    /// <summary>
    /// funding data is changed
    /// </summary>
    public event Action<Funding> FundingChangedEvent;

    /// <summary>
    /// volumes 24h data is changed
    /// </summary>
    public event Action<SecurityVolumes> NewVolume24hChangedEvent;
}
