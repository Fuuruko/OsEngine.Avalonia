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

[Name("Heiken Ashi")]
public class HeikenAshi : ACandlesSeriesRealization
{
    public CandlesParameterString TimeFrameParameter;

    public TimeFrame TimeFrame
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                TimeFrameSpan = (TimeSpan)value.GetTimeSpan();
            }
        }
    }

    public TimeSpan TimeFrameSpan { get; private set; }

    public override void OnStateChange(CandleSeriesState state)
    {
        if (state == CandleSeriesState.Configure)
        {
            List<string> allTimeFrames =
            [
                TimeFrame.Sec1.ToString(),
                TimeFrame.Sec2.ToString(),
                TimeFrame.Sec5.ToString(),
                TimeFrame.Sec10.ToString(),
                TimeFrame.Sec15.ToString(),
                TimeFrame.Sec20.ToString(),
                TimeFrame.Sec30.ToString(),
                TimeFrame.Min1.ToString(),
                TimeFrame.Min2.ToString(),
                TimeFrame.Min3.ToString(),
                TimeFrame.Min5.ToString(),
                TimeFrame.Min10.ToString(),
                TimeFrame.Min15.ToString(),
                TimeFrame.Min20.ToString(),
                TimeFrame.Min30.ToString(),
                TimeFrame.Min45.ToString(),
                TimeFrame.Hour1.ToString(),
                TimeFrame.Hour2.ToString(),
                TimeFrame.Hour4.ToString(),
                TimeFrame.Day.ToString(),
            ];

            TimeFrameParameter
                 = CreateParameterStringCollection("TimeFrame",
                OsLocalization.Market.Label10, TimeFrame.Min30.ToString(), allTimeFrames);

            TimeFrame = TimeFrame.Min30;
        }
        else if (state == CandleSeriesState.ParametersChange)
        {

            Enum.TryParse(TimeFrameParameter.ValueString, out TimeFrame newTf);
            TimeFrame = newTf;
            CandlesAll?.Clear();
        }
    }

    public override void UpDateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        if (CandlesAll.Count == 0)
        {
            PreUpdateCandle(time, price, volume, canPushUp, side);
            return;
        }

        // если пришли старые данные
        if (CandlesAll[^1].Time > time) { return; }


        if (CandlesAll[^1].Time < time &&
                CandlesAll[^1].Time.Add(TimeFrameSpan) <= time
                ||
                TimeFrame == TimeFrame.Day &&
                CandlesAll[^1].Time.Date < time.Date
           )
        {
            // если пришли данные из новой свечки
            FinishCandle(time, price, volume, canPushUp, side);
            return;
        }

        if (CandlesAll[^1].Time <= time &&
            CandlesAll[^1].Time.Add(TimeFrameSpan) > time)
        {
            // если пришли данные внутри свечи

            CandlesAll[^1].Volume += volume;

            if (CandlesAll[^1].High < price)
            {
                CandlesAll[^1].High = price;
            }

            if (CandlesAll[^1].Low > price)
            {
                CandlesAll[^1].Low = price;
            }

            CandlesAll[^1].Close = Math.Round((CandlesAll[^1].Open +
                                                      CandlesAll[^1].High +
                                                      CandlesAll[^1].Low +
                                                      CandlesAll[^1].Close) / 4, Security.Decimals);

            // NOTE: Thats impossible(?)
            // if (CandlesAll[^1].Close > CandlesAll[^1].High)
            // {
            //     CandlesAll[^1].High = CandlesAll[^1].Close;
            // }
            // if (CandlesAll[^1].Close < CandlesAll[^1].Low)
            // {
            //     CandlesAll[^1].Low = CandlesAll[^1].Close;
            // }

            if (canPushUp)
            {
                UpdateChangeCandle();
            }
        }
    }

    protected override void PreUpdateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        DateTime timeNextCandle = time;

        if (TimeFrameSpan.TotalMinutes >= 1)
        {
            timeNextCandle = time.AddSeconds(-time.Second);

            while (timeNextCandle.Minute % TimeFrameSpan.TotalMinutes != 0)
            {
                timeNextCandle = timeNextCandle.AddMinutes(-1);
            }

            while (timeNextCandle.Second != 0)
            {
                timeNextCandle = timeNextCandle.AddSeconds(-1);
            }
        }
        else
        {
            while (timeNextCandle.Second % TimeFrameSpan.TotalSeconds != 0)
            {
                timeNextCandle = timeNextCandle.AddSeconds(-1);
            }
        }

        while (timeNextCandle.Millisecond != 0)
        {
            timeNextCandle = timeNextCandle.AddMilliseconds(-1);
        }

        timeNextCandle = new DateTime(timeNextCandle.Year, timeNextCandle.Month, timeNextCandle.Day,
                timeNextCandle.Hour, timeNextCandle.Minute, timeNextCandle.Second, timeNextCandle.Millisecond);

        Candle candle = new()
        {
            Close = price,
            High = price,
            Low = price,
            Open = price,
            State = CandleState.Started,
            Time = timeNextCandle,
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
        CandlesAll[^1].Close = Math.Round((CandlesAll[^1].Open +
                    CandlesAll[^1].High +
                    CandlesAll[^1].Low +
                    CandlesAll[^1].Close) / 4, Security.Decimals);

        // NOTE: Thats impossible(?)
        // if (CandlesAll[^1].Close > CandlesAll[^1].High)
        // {
        //     CandlesAll[^1].High = CandlesAll[^1].Close;
        // }
        // if (CandlesAll[^1].Close < CandlesAll[^1].Low)
        // {
        //     CandlesAll[^1].Low = CandlesAll[^1].Close;
        // }


        if (CandlesAll[^1].State != CandleState.Finished)
        {
            // если последнюю свечку ещё не закрыли и не отправили
            CandlesAll[^1].State = CandleState.Finished;

            if (canPushUp)
            {
                UpdateFinishCandle();
            }
        }

        DateTime timeNextCandle = time;

        if (TimeFrameSpan.TotalMinutes >= 1)
        {
            timeNextCandle = time.AddSeconds(-time.Second);

            while (timeNextCandle.Minute % TimeFrameSpan.TotalMinutes != 0 &&
                    TimeFrame != TimeFrame.Min45 && TimeFrame != TimeFrame.Min3)
            {
                timeNextCandle = timeNextCandle.AddMinutes(-1);
            }

            while (timeNextCandle.Second != 0)
            {
                timeNextCandle = timeNextCandle.AddSeconds(-1);
            }
        }
        else
        {
            while (timeNextCandle.Second % TimeFrameSpan.TotalSeconds != 0)
            {
                timeNextCandle = timeNextCandle.AddSeconds(-1);
            }
        }

        while (timeNextCandle.Millisecond != 0)
        {
            timeNextCandle = timeNextCandle.AddMilliseconds(-1);
        }

        timeNextCandle = new DateTime(timeNextCandle.Year, timeNextCandle.Month, timeNextCandle.Day,
                timeNextCandle.Hour, timeNextCandle.Minute, timeNextCandle.Second, timeNextCandle.Millisecond);

        decimal startVal = Math.Round((CandlesAll[^1].Open +
                    CandlesAll[^1].Close) / 2, Security.Decimals);

        Candle newCandle = new()
        {
            Time = timeNextCandle,
            Open = startVal,
            High = startVal,
            Low = startVal,
            Close = startVal,
            Volume = volume
        };

        CandlesAll.Add(newCandle);

        if (canPushUp)
        {
            UpdateChangeCandle();
        }

    }
}
