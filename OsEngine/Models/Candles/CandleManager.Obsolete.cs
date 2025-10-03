using System;

namespace OsEngine.Models.Candles;

public partial class CandleManager
{
    [Obsolete($"Use {nameof(CandleUpdated)} instead")]
    public event Action<CandleSeries> CandleUpdateEvent
    {
        add => CandleUpdated += value;
        remove => CandleUpdated -= value;
    }
}
