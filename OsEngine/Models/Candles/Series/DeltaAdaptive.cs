/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Language;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;
using System;

namespace OsEngine.Models.Candles.Series;

[Name("Delta Adaptive")]
public class DeltaAdaptive : ACandlesSeriesRealization
{
    public CandlesParameterDecimal DeltaPeriods;

    public CandlesParameterInt CandlesCountInDay;

    public CandlesParameterInt DaysLookBack;

    public override void OnStateChange(CandleSeriesState state)
    {
        if (state == CandleSeriesState.Configure)
        {
            DeltaPeriods = CreateParameterDecimal("DeltaPeriods", OsLocalization.Market.Label13, 10000m);
            CandlesCountInDay = CreateParameterInt("CandlesCountInDay", OsLocalization.Market.Label125, 100);
            DaysLookBack = CreateParameterInt("DaysLookBack", OsLocalization.Market.Label126, 1);
        }
        else if (state == CandleSeriesState.ParametersChange)
        {
            if(DaysLookBack.ValueInt > 2)
            {
                DaysLookBack.ValueInt = 2;
            }
            if (DaysLookBack.ValueInt <= 0)
            {
                DaysLookBack.ValueInt = 1;
            }
            if(CandlesCountInDay.ValueInt <= 0)
            {
                CandlesCountInDay.ValueInt = 1;
            }
        }
    }

    private void RebuildCandlesCount()
    {
        decimal candlesCount = 0;

        DateTime date = CandlesAll[^1].Time.Date;

        int days = 0;

        int i;
        for (i = CandlesAll.Count - 1; i >= 0; i--)
        {
            Candle curCandle = CandlesAll[i];

            if (curCandle.Time.Date < date)
            {
                date = curCandle.Time.Date;
                days++;
            }

            if (days >= DaysLookBack.ValueInt) { break; }

            candlesCount++;

        }

        if (i == 0) { days++; }

        if (candlesCount == 0) { return; }

        decimal countCandlesInDay = candlesCount / days;

        decimal commonChangeDelta = DeltaPeriods.ValueDecimal * countCandlesInDay;

        decimal newDelta = commonChangeDelta / CandlesCountInDay.ValueInt;

        DeltaPeriods.ValueDecimal = newDelta;
    }

    private decimal _currentDelta;

    public override void UpDateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        // Формула кумулятивной дельты 
        //Delta= ∑_i▒vBuy- ∑_i▒vSell 
        if (CandlesAll.Count == 0)
        {
            PreUpdateCandle(time, price, volume, canPushUp, side);
            return;
        }

        // если пришли старые данные
        if (CandlesAll[^1].Time > time) { return; }


        if (CandlesAll[^1].Time.Date < time.Date)
        {
            // пришли данные из нового дня
            RebuildCandlesCount();
            FinishCandle(time, price, volume, canPushUp, side);
            return;
        }


        _currentDelta += side == Side.Buy ? volume : -volume;

        if (Math.Abs(_currentDelta) >= DeltaPeriods.ValueDecimal)
        {
            // если пришли данные из новой свечки
            FinishCandle(time, price, volume, canPushUp, side);
            return;
        }

        // если пришли данные внутри свечи

        CandlesAll[^1].Volume += volume;
        CandlesAll[^1].Close = price;

        if (CandlesAll[^1].High < price)
        {
            CandlesAll[^1].High = price;
        }

        if (CandlesAll[^1].Low > price)
        {
            CandlesAll[^1].Low = price;
        }

        if (canPushUp)
        {
            UpdateChangeCandle();
        }
    }

    protected override void PreUpdateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        time = time.AddMilliseconds(-time.Millisecond);

        Candle candle = new()
        {
            Close = price,
            High = price,
            Low = price,
            Open = price,
            Time = time,
            Volume = volume
        };

        CandlesAll.Add(candle);

        if (canPushUp)
        {
            UpdateChangeCandle();
        }

        _currentDelta = 0;
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

            if (canPushUp)
            {
                UpdateFinishCandle();
            }
        }

        Candle newCandle = new()
        {
            Time = time.AddMilliseconds(-time.Millisecond),
            Open = price,
            High = price,
            Low = price,
            Close = price,
            Volume = volume
        };

        CandlesAll.Add(newCandle);

        if (canPushUp)
        {
            UpdateChangeCandle();
        }

    }
}
