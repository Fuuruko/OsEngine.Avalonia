using System;
using System.Collections.Generic;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Market.Servers;

/// <summary>
/// Blocks the display of the default server settings in the settings window. 
/// </summary>
// NOTE: Is it only FeedServer? if that change name or unify with IFeedServer
public interface IHideParameters
{
    // Used as marker that Parameters should be hidden
}

public interface IFeedServer
{
    // Used as marker that Server can load data
    public List<Trade> IGetTrades(Security security, DateTime startTime, DateTime endTime) { return null; }

    public List<Trade> IGetCandles(Security security, DateTime startTime, DateTime endTime) { return null; }
}

public interface INewsServer
{
    // Used as marker that Server can recieve news
}

internal interface INotStandardServerInitialization
{
    // Used as marker Server can have many proxies
    InitializationType InitializationType { get; }
}

// TODO: Instead of IsSupportProxy setting
internal interface IProxySupport
{

}

internal enum InitializationType
{

}
