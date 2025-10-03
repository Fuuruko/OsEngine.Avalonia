/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Generic;
using System.Net;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Candles;

namespace OsEngine.Models.Market.Servers
{
    /// <summary>
    /// connection implementation for AServer
    /// реализация подключения для AServer
    /// </summary>
    public abstract class BaseServerRealization : IServerRealization
    {
        public abstract ServerType ServerType { get; }

        /// <summary>
        /// server state
        /// состояние сервера
        /// </summary>
        [Obsolete($"Use {nameof(IsConnected)} instead")]
        public ServerConnectStatus ServerStatus { get; set; } = ServerConnectStatus.Disconnect;

        public bool IsConnected { get; set; } = false;

        public DateTime ServerTime { get; set; }

        public List<IServerParameter> ServerParameters { get; set; }

        /// <summary>
        /// request to connect to the source. guaranteed to be called no more than 60 seconds
        /// запрос подключения к источнику. гарантированно вызывается не чаще чем в 60 секунд
        /// </summary>
        public abstract void Connect(WebProxy proxy);

        /// <summary>
        /// dispose resources of API
        /// освободить ресурсы АПИ
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// API connection established
        /// соединение с API установлено
        /// </summary>
        public event Action ConnectEvent;

        /// <summary>
        /// API connection broke
        /// соединение с API разорвано
        /// </summary>
        public event Action DisconnectEvent;

        /// <summary>
        /// request security
        /// запросить бумаги
        /// </summary>
        public abstract void GetSecurities();

        /// <summary>
        /// new securities in the system
        /// новые бумаги в системе
        /// </summary>
        public event Action<List<Security>> SecurityEvent;

        /// <summary>
        /// request portfolios
        /// запросить портфели
        /// </summary>
        public abstract void GetPortfolios();

        /// <summary>
        /// portfolios updates
        /// обновились портфели
        /// </summary>
        public event Action<List<Portfolio>> PortfolioEvent;

        /// <summary>
        /// subscribe to trades and market depth
        /// подписаться на трейды и стаканы
        /// </summary>
        public abstract void Subscrible(Security security);

        /// <summary>
        /// subscribe to news
        /// </summary>
        public abstract bool SubscribeNews();

        /// <summary>
        /// the news has come out
        /// </summary>
        public event Action<News> NewsEvent;

        /// <summary>
        /// depth updated
        /// обновился стакан
        /// </summary>
        public event Action<MarketDepth> MarketDepthEvent;

        /// <summary>
        /// ticks updated
        /// обновились тики
        /// </summary>
        public event Action<Trade> NewTradesEvent;

        /// <summary>
        /// Интерфейс для получения последний свечек по инструменту. Используется для активации серий свечей в боевых торгах
        /// Interface for getting the last candlesticks for a security. Used to activate candlestick series in live trades
        /// </summary>
        public abstract List<Candle> GetLastCandleHistory(Security security, TimeFrameBuilder timeFrameBuilder, int candleCount);

        /// <summary>
        /// take candles history for period
        /// взять историю свечей за период
        /// </summary>
        public abstract List<Candle> GetCandleDataToSecurity(Security security, TimeFrameBuilder timeFrameBuilder,
                DateTime startTime, DateTime endTime, DateTime actualTime);

        /// <summary>
        /// take ticks data for period
        /// взять тиковые данные за период
        /// </summary>
        public abstract List<Trade> GetTickDataToSecurity(Security security, DateTime startTime, DateTime endTime, DateTime actualTime);

        /// <summary>
        /// place order
        /// исполнить ордер
        /// </summary>
        public abstract void SendOrder(Order order);

        /// <summary>
        /// Order price change
        /// </summary>
        /// <param name="order">An order that will have a new price</param>
        /// <param name="newPrice">New price</param>
        public abstract void ChangeOrderPrice(Order order, decimal newPrice);

        /// <summary>
        /// cancel order
        /// отозвать ордер
        /// </summary>
        public abstract bool CancelOrder(Order order);

        /// <summary>
        /// cancel all orders from trading system
        /// отозвать все ордера из торговой системы
        /// </summary>
        public abstract  void CancelAllOrders();

        /// <summary>
        /// cancel all orders from trading system to security
        /// отозвать все ордера из торговой системы по названию инструмента
        /// </summary>
        public abstract void CancelAllOrdersToSecurity(Security security);

        /// <summary>
        /// Query list of orders that are currently in the market
        /// </summary>
        public abstract void GetAllActivOrders();

        /// <summary>
        /// Query order status
        /// </summary>
        public abstract OrderStateType GetOrderStatus(Order order);

        /// <summary>
        /// новые мои ордера
        /// my new orders
        /// </summary>
        public event Action<Order> MyOrderEvent;

        /// <summary>
        /// my new trades
        /// новые мои сделки
        /// </summary>
        public event Action<MyTrade> MyTradeEvent;

        /// <summary>
        /// send the message
        /// отправляет сообщение
        /// </summary>
        public event Action<string, LogMessageType> LogMessageEvent;

        /// <summary>
        /// additional market data
        /// дополнительные маркет данные по тикеру
        /// </summary>
        public event Action<OptionMarketDataForConnector> AdditionalMarketDataEvent;
    }
}
