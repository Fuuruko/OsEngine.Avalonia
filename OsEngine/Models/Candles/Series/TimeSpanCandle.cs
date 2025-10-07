/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Language;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles.Series;

[Name("TimeSpan Candle")]
public class TimeSpanCandle : ACandlesSeriesRealization
{
    public CandlesParameterInt Hours;

    public CandlesParameterInt Minutes;

    public CandlesParameterInt Seconds;

    public CandlesParameterString ForcedStartFromZero;

    public override void OnStateChange(CandleSeriesState state)
    {
        if (state == CandleSeriesState.Configure)
        {
            List<string> workRegimes = ["Every day", "Every hour", "Every minute", "Off"];

            ForcedStartFromZero
                = CreateParameterStringCollection("ForcedStartFromZero",
               OsLocalization.Market.Label124, "Every hour", workRegimes);

            Hours = CreateParameterInt("Hours", "Hours", 0);
            Minutes = CreateParameterInt("Minutes", "Minutes", 59);
            Seconds = CreateParameterInt("Seconds", "Seconds", 40);

            TimeFrameSpan = new TimeSpan(Hours.ValueInt, Minutes.ValueInt, Seconds.ValueInt);
        }
        else if (state == CandleSeriesState.ParametersChange)
        {
            TimeFrameSpan = new TimeSpan(Hours.ValueInt, Minutes.ValueInt, Seconds.ValueInt);

            CandlesAll?.Clear();
        }
    }

    public TimeSpan TimeFrameSpan { get; private set; }

    public override void UpDateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        if (CandlesAll.Count == 0)
        {
            PreUpdateCandle(time, price, volume, canPushUp, side);
            return;
        }

        // если пришли старые данные
        if (CandlesAll[^1].Time > time) { return; }

        if (
              CandlesAll[^1].Time < time &&
              CandlesAll[^1].Time.Add(TimeFrameSpan) <= time
            ||
              ForcedStartFromZero.ValueString == "Every day" &&
              CandlesAll[^1].Time.Day != time.Day
            ||
              ForcedStartFromZero.ValueString == "Every hour" &&
              CandlesAll[^1].Time.Hour != time.Hour
            ||
              ForcedStartFromZero.ValueString == "Every minute" &&
              CandlesAll[^1].Time.Minute != time.Minute
            )
        {
            // если пришли данные из новой свечки

            FinishCandle(time, price, volume,canPushUp, side);
            return;
        }

        if (CandlesAll[^1].Time.Add(TimeFrameSpan) > time)
        {
            // если пришли данные внутри свечи

            if (CandlesAll[^1].State == CandleState.Finished)
            {
                CandlesAll[^1].State = CandleState.Started;
            }

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
    }

    protected override void PreUpdateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        Candle candle = new()
        {
            Time = time.AddMilliseconds(-time.Millisecond),
            Open = price,
            High = price,
            Low = price,
            Close = price,
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
