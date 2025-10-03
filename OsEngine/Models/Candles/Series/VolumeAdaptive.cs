/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using OsEngine.Language;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles.Series;

[Candle("VolumeAdaptive")]
public class VolumeAdaptive : ACandlesSeriesRealization
{
    // [Parameter]
    // public Decimal VolumeToCloseCandle = new("Volume to close", 10000);
    public CandlesParameterDecimal VolumeToCloseCandle;

    // [Parameter]
    // public Int CandlesCountInDay = new("Candles count in day", 100);
    public CandlesParameterInt CandlesCountInDay;

    // [Parameter]
    // public Int DaysLookBack = new("Candles count in day", 100);
    public CandlesParameterInt DaysLookBack;

    // public VolumeAdaptive()
    // {
    //     DaysLookBack.OnValueChanged += delegate
    //     {
    //
    //     }
    // }

    public override void OnStateChange(CandleSeriesState state)
    {
        if (state == CandleSeriesState.Configure)
        {
            // VolumeToCloseCandle = CreateParameterDecimal("VolumeToCloseCandle", OsLocalization.Market.Label14, 10000m);
            // CandlesCountInDay = CreateParameterInt("CandlesCountInDay", OsLocalization.Market.Label125, 100);
            // DaysLookBack = CreateParameterInt("DaysLookBack", OsLocalization.Market.Label126, 1);
        }
        else if (state == CandleSeriesState.ParametersChange)
        {
            if (DaysLookBack.ValueInt <= 0)
            {
                DaysLookBack.ValueInt = 1;
            }
            if (CandlesCountInDay.ValueInt <= 0)
            {
                CandlesCountInDay.ValueInt = 1;
            }
            if (VolumeToCloseCandle.ValueDecimal <= 0)
            {
                VolumeToCloseCandle.ValueDecimal = 1;
            }
        }
    }

    private void RebuildCandlesCount()
    {
        decimal volumeOnLastDay = 0;

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

            volumeOnLastDay += curCandle.Volume;

        }
        if (i == 0) { days++; }

        if (volumeOnLastDay == 0) { return; }

        decimal volumeInOneCandle = volumeOnLastDay / (CandlesCountInDay.ValueInt * days);

        VolumeToCloseCandle.ValueDecimal = volumeInOneCandle;
    }

    public override void UpDateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {

        if (CandlesAll.Count == 0)
        {
            PreUpdateCandle(time, price, volume, canPushUp, side);
            return;
        }

        var lastCandle = CandlesAll[^1];
        // если пришли старые данные
        if (lastCandle.Time > time) { return; }

        if (lastCandle.Time.Date < time.Date)
        {
            // пришли данные из нового дня
            RebuildCandlesCount();
            FinishCandle(time, price, volume, canPushUp, side);
            return;
        }

        if (lastCandle.Volume >= VolumeToCloseCandle.ValueDecimal)
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

        if (canPushUp)
        {
            UpdateChangeCandle();
        }
    }

    protected override void PreUpdateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        Candle candle = new()
        {
            Time = time.AddMilliseconds(-time.Millisecond),
            Close = price,
            High = price,
            Low = price,
            Open = price,
            Volume = volume
        };

        CandlesAll.Add(candle);

        if (canPushUp)
        {
            UpdateChangeCandle();
        }
    }

    protected override void UpdateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        throw new NotImplementedException();
    }

    protected override void FinishCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
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
