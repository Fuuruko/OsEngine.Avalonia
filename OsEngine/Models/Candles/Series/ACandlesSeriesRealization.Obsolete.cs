/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles.Series;

// NOTE: Rename?
public abstract partial class ACandlesSeriesRealization
{
    [Obsolete($"Use {nameof(UpdateCandle)} instead")]
    public abstract void UpDateCandle(DateTime time, decimal price,
            decimal volume, bool canPushUp, Side side);

    [Obsolete(nameof(OnCandleUpdated))]
    public void UpdateChangeCandle() => CandleUpdated?.Invoke();
    [Obsolete(nameof(OnCandleFinished))]
    public void UpdateFinishCandle() => CandleFinished?.Invoke();

    [Obsolete(nameof(CandleUpdated))]
    public event Action CandleUpdateEvent
    {
        add { CandleUpdated += value; }
        remove { CandleUpdated -= value; }
    }

    [Obsolete(nameof(CandleFinished))]
    public event Action CandleFinishedEvent
    {
        add { CandleFinished += value; }
        remove { CandleFinished -= value; }
    }
}
