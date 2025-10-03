/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using OsEngine.Language;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles.Series;

[Candle("Delta")]
public class Delta : ACandlesSeriesRealization
{
    private decimal _currentDelta;

    public CandlesParameterDecimal DeltaPeriods;

    public override void OnStateChange(CandleSeriesState state)
    {
        if (state == CandleSeriesState.Configure)
        {
            DeltaPeriods = CreateParameterDecimal("DeltaPeriods", OsLocalization.Market.Label13, 10000m);
        }
        else if (state == CandleSeriesState.ParametersChange)
        {
            CandlesAll?.Clear();
        }
    }

    public override void UpDateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        // Формула кумулятивной дельты 
        //Delta= ∑_i▒vBuy- ∑_i▒vSell 
        if (CandlesAll.Count == 0)
        {
            PreUpdateCandle(time, price, volume, canPushUp, side);
            return;
        }

        Candle lastCandle = CandlesAll[^1];

        // если пришли старые данные
        if (lastCandle.Time > time) { return; }

        _currentDelta += side == Side.Buy ? volume : -volume;

        if (Math.Abs(_currentDelta) >= DeltaPeriods.ValueDecimal)
        {
            // если пришли данные из новой свечки
            FinishCandle(time, price, volume, canPushUp, side);
            return;
        }

        // если пришли данные внутри свечи

        lastCandle.Volume += volume;
        lastCandle.Close = price;

        if (lastCandle.High < price)
        {
            lastCandle.High = price;
        }

        if (lastCandle.Low > price)
        {
            lastCandle.Low = price;
        }

        OnCandleUpdated(canPushUp);
    }

    protected override void PreUpdateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        time = time.AddMilliseconds(-time.Millisecond);

        Candle candle = new()
        {
            Time = time,
            High = price,
            Open = price,
            Close = price,
            Low = price,
            Volume = volume
        };

        CandlesAll.Add(candle);

        OnCandleUpdated(canPushUp);
    }

    protected override void UpdateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        throw new NotImplementedException();
    }

    protected override void FinishCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        _currentDelta = 0;

        if (CandlesAll[^1].State != CandleState.Finished)
        {
            // если последнюю свечку ещё не закрыли и не отправили
            CandlesAll[^1].State = CandleState.Finished;

            OnCandleFinished(canPushUp);
        }

        time = time.AddMilliseconds(-time.Millisecond);

        Candle newCandle = new()
        {
            Close = price,
            High = price,
            Low = price,
            Open = price,
            State = CandleState.Started,
            Time = time,
            Volume = volume
        };

        CandlesAll.Add(newCandle);

        OnCandleUpdated(canPushUp);
    }
}
