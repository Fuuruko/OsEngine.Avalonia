namespace OsEngine.Models.Market.Servers.Finam;

public partial class Finam
{
    public override ServerPermissions Permissions =>
        ServerPermissions.Get(ServerType.Finam)
        ?? new(ServerType.Finam)
        {
            LoadableTimeFrames = new()
            {
                Sec1 = true,
                Sec2 = true,
                Sec5 = true,
                Sec10 = true,
                Sec15 = true,
                Sec30 = true,
                Min1 = true,
                Min2 = false,
                Min5 = true,
                Min10 = true,
                Min15 = true,
                Min30 = true,
                Hour1 = true,
                Hour2 = false,
                Hour4 = false,
                Day = true,

                Tick = true,
                MarketDepth = false,
            },

            SupportsMarketOrders = false,
            CanChangeOrderPrice = false,
            UsesLotToCalculateProfit = false,
            UsesStandardCandlesStarter = false,

            SecondsAfterStartSendOrders = 60,

            IsManuallyClosePositionOnBoardEnabled = false,
            ManuallyClosePositionOnBoardTrimmedNames = null,
            ManuallyClosePositionOnBoard_ExceptionPositionNames = null,

            CanQueryOrdersAfterReconnect = false,
            CanQueryOrderStatus = false,

            IsNewsServer = false,

            SupportsCheckDataFeedLogic = false,
            CheckDataFeedLogic_ExceptionSecurities = null,
            CheckDataFeedLogic_NoDataMinutesToDisconnect = 10,

            SupportsMultipleServers = false,
            SupportsProxyForMultipleServers = false,

            SupportsAsyncOrderSending = false,
            AsyncOrderSending_RateGateLimitMls = 10,

            EnabledTimeFrames = new()
            {
                Sec1  = false,
                Sec2  = false,
                Sec5  = false,
                Sec10 = false,
                Sec15 = false,
                Sec20 = false,
                Sec30 = false,
                Min1  = false,
                Min2  = false,
                Min3  = false,
                Min5  = false,
                Min10 = false,
                Min15 = false,
                Min20 = false,
                Min30 = false,
                Min45 = false,
                Hour1 = false,
                Hour2 = false,
                Hour4 = false,
                Day   = false,
            }

        };
}
