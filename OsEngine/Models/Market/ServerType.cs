namespace OsEngine.Models.Market
{
    /// <summary>
    /// type of connection to trading. Server type
    /// тип подключения к торгам. Тип сервера
    /// </summary>
    public enum ServerType
    {
        /// <summary>
        /// server type not defined
        /// Тип сервера не назначен
        /// </summary>
        None,

        /// <summary>
        /// connection to Russian broker T-Invest
        /// подключение к Т-Инвестициям (версия 3 коннектора)
        /// </summary>
        TInvest,

        /// <summary>
        /// cryptocurrency exchange Hitbtc
        /// биржа криптовалют Hitbtc
        /// </summary>
        Hitbtc,

        /// <summary>
        /// cryptocurrency exchange Gate.io
        /// биржа криптовалют Gate.io
        /// </summary>
        GateIoSpot,

        /// <summary>
        /// Futures of cryptocurrency exchange Gate.io
        /// Фьючерсы биржи криптовалют Gate.io
        /// </summary>
        GateIoFutures,

        /// <summary>
        /// cryptocurrency exchange ZB
        /// биржа криптовалют ZB
        /// </summary>
        Zb,

        /// <summary>
        /// BitMax exchange
        /// биржа BitMax
        /// </summary>
        AscendEx_BitMax,

        /// <summary>
        /// transaq
        /// транзак
        /// </summary>
        Transaq,

        /// <summary>
        /// LMax exchange
        /// биржа LMax
        /// </summary>
        Lmax,

        /// <summary>
        /// cryptocurrency exchange BitfinexSpot
        /// биржа криптовалют BitfinexSpot
        /// </summary>
        BitfinexSpot,

        /// <summary>
        /// cryptocurrency exchange Binance
        /// биржа криптовалют Binance
        /// </summary>
        Binance,

        /// <summary>
        /// cryptocurrency exchange Binance Futures
        /// биржа криптовалют Binance, секция фьючеры
        /// </summary>
        BinanceFutures,

        /// <summary>
        /// cryptocurrency exchange Exmo
        /// биржа криптовалют Exmo
        /// </summary>
        Exmo,

        /// <summary>
        /// terminal Ninja Trader
        /// нинзя трейдер
        /// </summary>
        NinjaTrader,

        /// <summary>
        /// cryptocurrency exchange Kraken
        /// биржа криптовалют Kraken
        /// </summary>
        Kraken,

        /// <summary>
        /// cryptocurrency exchange BitMEX
        /// биржа криптовалют BitMEX
        /// </summary>
        BitMex,

        /// <summary>
        /// cryptocurrency exchange BitStamp
        /// биржа криптовалют BitStamp
        /// </summary>
        BitStamp,

        /// <summary>
        /// optimizer
        /// Оптимизатор
        /// </summary>
        Optimizer,

        /// <summary>
        /// connection to terminal Quik by LUA
        /// Квик луа
        /// </summary>
        QuikLua,

        /// <summary>
        /// connection to terminal Quik by DDE
        /// Квик
        /// </summary>
        QuikDde,

        /// <summary>
        /// Plaza 2
        /// Плаза 2
        /// </summary>
        Plaza,

        /// <summary>
        /// Tester
        /// Тестер
        /// </summary>
        Tester,

        /// <summary>
        /// IB
        /// </summary>
        InteractiveBrokers,

        /// <summary>
        /// Finam
        /// Финам
        /// </summary>
        Finam,

        /// <summary>
        /// AstsBridge, he is also the gateway or TEAP
        /// AstsBridge, он же ШЛЮЗ, он же TEAP 
        /// </summary>
        AstsBridge,

        /// <summary>
        /// Дата сервер московской биржи
        /// </summary>
        MoexDataServer,

        /// <summary>
        /// MFD web server
        /// </summary>
        MfdWeb,

        /// <summary>
        /// Bybit exchange
        /// </summary>
        Bybit,

        /// <summary>
        /// OKX exchange
        /// </summary>
        OKX,

        /// <summary>
        /// Ascendex exchange
        /// </summary>
        Bitmax_AscendexFutures,

        /// <summary>
        /// BitGetSpot exchange
        /// </summary>
        BitGetSpot,

        /// <summary>
        /// BitGetFutures exchange
        /// </summary>
        BitGetFutures,

        /// <summary>
        /// Alor OpenAPI & Websocket
        /// </summary>
        Alor,

        /// <summary>
        /// KuCoinSpot exchange
        /// </summary>
        KuCoinSpot,

        /// <summary>
        /// KuCoinSpot exchange
        /// </summary>
        KuCoinFutures,

        /// <summary>
        /// BingXSpot exchange
        /// </summary>
        BingXSpot,

        /// <summary>
        /// BingXFutures exchange
        /// </summary>
        BingXFutures,

        /// <summary>
        /// Deribit exchange
        /// </summary>
        Deribit,

        /// <summary>
        /// XT Spot exchange
        /// </summary>
        XTSpot,

        /// <summary>
        /// Pionex exchange
        /// </summary>
        PionexSpot,

        /// <summary>
        /// Woo exchange
        /// </summary>
        Woo,

        /// <summary>
        /// MoexAlgopack data-server
        /// </summary>
        MoexAlgopack,

        /// <summary>
        /// HTXSpot exchange
        /// </summary>
        HTXSpot,

        /// <summary>
        /// HTXFutures exchange
        /// </summary>
        HTXFutures,

        /// <summary>
        /// HTXSwap exchange
        /// </summary>
        HTXSwap,

        /// <summary>
        /// FIX/FAST for MOEX Spot
        /// </summary>
        MoexFixFastSpot,

        /// BitMart Spot exchange
        /// </summary>
        BitMart,

        /// BitMart Futures exchange
        /// </summary>
        BitMartFutures,

        /// <summary>
        /// FIX/FAST for MOEX Currency
        /// </summary>
        MoexFixFastCurrency,

        /// <summary>
        /// FIX/FAST/TWIME for MOEX Futures
        /// </summary>
        MoexFixFastTwimeFutures,

        /// <summary>
        /// TraderNet
        /// </summary>
        TraderNet,

        /// <summary>
        /// Mexc Spot
        /// </summary>
        MexcSpot,

        /// <summary>
        /// Mexc Spot
        /// </summary>
        KiteConnect,

        /// <summary>
        /// Yahoo Finance
        /// </summary>
        YahooFinance,

        /// <summary>
        /// ATPlatform
        /// </summary>
        Atp,

        /// <summary>
        /// Polygon.io
        /// </summary>
        Polygon,

        /// <summary>
        /// Spot for cryptocurrency exchange CoinEx.com
        /// Спот биржи криптовалют CoinEx.com
        /// </summary>
        CoinExSpot,

        /// <summary>
        /// Futures for cryptocurrency exchange CoinEx.com
        /// Фьюючерсы биржи криптовалют CoinEx.com
        /// </summary>
        CoinExFutures,

        /// <summary>
        /// Reading news from RSS feeds
        /// Чтение новостей с RSS лент
        /// </summary>
        RSSNews,

        /// <summary>
        /// Reading news from smart-lab.ru
        /// Чтение новостей с сайта smart-lab.ru
        /// </summary>
        SmartLabNews,

        /// <summary>
        /// Options exchange Alternative Exchange ae.exchange
        /// Опционная биржа Alternative Exchange ae.exchange
        /// </summary>
        AExchange,

        /// <summary>
        /// BloFinFutures exchange
        /// </summary>
        BloFinFutures,
    }
}
