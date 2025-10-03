using System;
using System.Collections.Generic;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Market.Servers.Tester;

public partial class TesterServer
{
    public bool RemoveTradesFromMemory
    {
        get => _removeTradesFromMemory;
        set
        {
            if (value == _removeTradesFromMemory)
            {
                return;
            }

            _removeTradesFromMemory = value;
            Save();
        }
    }
    private bool _removeTradesFromMemory;


    public DateTime LastStartServerTime { get; set; }

    public void StartServer() { }

    public void StopServer() { }

    public void ChangeOrderPrice(Order order, decimal newPrice) {  }

    public void CancelAllOrders(Security security = null) {  }

    public List<Candle> GetCandleDataToSecurity(string securityName, string securityClass, TimeFrameBuilder timeFrameBuilder,
            DateTime startTime, DateTime endTime, DateTime actualTime, bool needToUpdate)
    {
        return null;
    }

    public List<Trade> GetTickDataToSecurity(string securityName, string securityClass, DateTime startTime, DateTime endTime, DateTime actualTime, bool needToUpdete)
    {
        return null;
    }

    public List<Candle> GetLastCandleHistory(Security security, TimeFrameBuilder timeFrameBuilder)
    {
        return null;
    }

    public bool SubscribeNews()
    {
        return false;
    }

    public void ShowDialog(int num = 0) { }

    public event Action<Funding> FundingUpdateEvent;
    public event Action<Funding> NewFundingEvent;

    public event Action<SecurityVolumes> Volume24hUpdateEvent;
    public event Action<SecurityVolumes> NewVolume24hUpdateEvent;
}
