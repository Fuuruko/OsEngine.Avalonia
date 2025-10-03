// #nullable enable
using System.Collections.Generic;

namespace OsEngine.Models.Market;

public class ServerPermissions
{
    public static readonly Dictionary<ServerType, ServerPermissions> serverPermissions = [];

    public required LoadableTimeFrames LoadableTimeFrames { get; init; } = null;
    public required EnabledTimeFrames EnabledTimeFrames { get; init; }

    #region Trade Permissions

    // NOTE: 50/50
    public required bool SupportsMarketOrders { get; init; } = false;
    // NOTE: 70/30
    public required bool CanChangeOrderPrice { get; init; } = false;
    public required bool UsesLotToCalculateProfit { get; init; } = false;
    // NOTE: Check if can be removed
    // NOTE: Use IMarker maybe?
    public required bool UsesStandardCandlesStarter { get; init; } = true;

    public required int SecondsAfterStartSendOrders { get; init; }

    public required bool IsManuallyClosePositionOnBoardEnabled { get; init; } = false;
    public string[] ManuallyClosePositionOnBoardTrimmedNames { get; init; } = null;
    public string[] ManuallyClosePositionOnBoard_ExceptionPositionNames { get; init; } = null;

    // NOTE: Rename to IsQueryOrdersAfterReconnectDisabled or smth negative
    public required bool CanQueryOrdersAfterReconnect { get; init; }
    // NOTE: Rename to IsQueryOrderStatusDisabled or smth negative
    public required bool CanQueryOrderStatus { get; init; }

    #endregion

    #region Other Permissions

    // TODO: Split NewsServer from Server
    public bool IsNewsServer { get; init; } = false;

    public required bool SupportsCheckDataFeedLogic { get; init; } = false;
    // NOTE: Rename
    // NOTE: Looks like next 2 properties used only default values
    public required string[]? CheckDataFeedLogic_ExceptionSecurities { get; init; } = null;
    public int CheckDataFeedLogic_NoDataMinutesToDisconnect { get; init; } = 10;

    public required bool SupportsMultipleServers { get; init; }
    public required bool SupportsProxyForMultipleServers { get; init; } = false;

    public bool SupportsAsyncOrderSending { get; init; } = false;
    public int AsyncOrderSending_RateGateLimitMls { get; init; } = 10;
    // NOTE: Unify SupportsAsyncOrderSending and AsyncOrderSending_RateGateLimitMls
    public int? AsyncOrderSendingDelay { get; init; } = null;


    #endregion

    public ServerPermissions(ServerType type) => serverPermissions.Add(type, this);

    public static ServerPermissions Get(ServerType type) =>
        serverPermissions.GetValueOrDefault(type, null);
}

public class EnabledTimeFrames
{
    public bool Sec1 { get; init; } = false;
    public bool Sec2 { get; init; } = false;
    public bool Sec5 { get; init; } = false;
    public bool Sec10 { get; init; } = false;
    public bool Sec15 { get; init; } = false;
    public bool Sec20 { get; init; } = false;
    public bool Sec30 { get; init; } = false;
    public bool Min1 { get; init; } = false;
    public bool Min2 { get; init; } = false;
    public bool Min3 { get; init; } = false;
    public bool Min5 { get; init; } = false;
    public bool Min10 { get; init; } = false;
    public bool Min15 { get; init; } = false;
    public bool Min20 { get; init; } = false;
    public bool Min30 { get; init; } = false;
    public bool Min45 { get; init; } = false;
    public bool Hour1 { get; init; } = false;
    public bool Hour2 { get; init; } = false;
    public bool Hour4 { get; init; } = false;
    public bool Day { get; init; } = false;
}

public class LoadableTimeFrames(bool defaultValue = false)
{
    public bool Sec1 { get; init; } = defaultValue;
    public bool Sec2 { get; init; } = defaultValue;
    public bool Sec5 { get; init; } = defaultValue;
    public bool Sec10 { get; init; } = defaultValue;
    public bool Sec15 { get; init; } = defaultValue;
    public bool Sec20 { get; init; } = defaultValue;
    public bool Sec30 { get; init; } = defaultValue;
    public bool Min1 { get; init; } = defaultValue;
    public bool Min2 { get; init; } = defaultValue;
    public bool Min5 { get; init; } = defaultValue;
    public bool Min10 { get; init; } = defaultValue;
    public bool Min15 { get; init; } = defaultValue;
    public bool Min30 { get; init; } = defaultValue;
    public bool Hour1 { get; init; } = defaultValue;
    public bool Hour2 { get; init; } = defaultValue;
    public bool Hour4 { get; init; } = defaultValue;
    public bool Day { get; init; } = defaultValue;

    public bool MarketDepth { get; init; } = defaultValue;
    public bool Tick { get; init;} = defaultValue;
}
