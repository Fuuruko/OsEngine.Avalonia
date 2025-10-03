using System;
using System.Collections.Generic;
using System.Net;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;

namespace OsEngine.Models.Market.Servers;

public partial class BaseServer
{
    public abstract ServerPermissions Permissions { get; }

    // NOTE: Can use IName convention like IConnect, ISubscribe etc
    // or Impl suffix
    public abstract void Connect(WebProxy proxy);
    public abstract void Dispose();

    // NOTE: Why return void if it Get?
    public abstract List<Security> GetSecurities();
    public abstract List<Portfolio> GetPortfolios();

    // TODO: Create wrapper with try-catch
    public abstract void ISubscribe(Security security);
    public abstract void ISubscribeNews();

    /// <summary>
    /// Интерфейс для получения последний свечек по инструменту. Используется для активации серий свечей в боевых торгах
    /// Interface for getting the last candlesticks for a security. Used to activate candlestick series in live trades
    /// </summary>
    protected abstract List<Candle> IGetLastCandleHistory(Security security, TimeFrame timeFrame, int candleCount);

    /// <summary>
    /// take candles history for period
    /// взять историю свечей за период
    /// </summary>
    protected abstract List<Candle> IGetCandles(Security security, TimeFrame timeFrame,
            DateTime startTime, DateTime endTime, DateTime actualTime);

    /// <summary>
    /// take ticks data for period
    /// взять тиковые данные за период
    /// </summary>
    protected abstract List<Trade> IGetTrades(Security security, DateTime startTime, DateTime endTime, DateTime actualTime);

    /// <summary>
    /// place order
    /// исполнить ордер
    /// </summary>
    // NOTE: Rename maybe? Summary and name doesnt exactly the same
    protected abstract void ISendOrder(Order order);

    /// <summary>
    /// Order price change
    /// </summary>
    /// <param name="order">An order that will have a new price</param>
    /// <param name="newPrice">New price</param>
    protected abstract void IChangeOrderPrice(Order order, decimal newPrice);

    protected abstract bool ICancelOrder(Order order);

    // TODO: Merge to one function
    protected abstract void ICancelAllOrders(Security security = null);

    protected abstract void ICancelAllOrdersToSecurity(Security security);

    /// <summary>
    /// Query list of orders that are currently in the market
    /// </summary>
    protected abstract List<Order> IGetActiveOrders();

    /// <summary>
    /// Query order status
    /// </summary>
    protected abstract OrderStateType IGetOrderStatus(Order order);
}
