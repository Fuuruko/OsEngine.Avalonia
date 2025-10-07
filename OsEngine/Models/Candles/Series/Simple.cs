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

public class Simple : ACandlesSeriesRealization
{
    public CandlesParameterString TimeFrameParameter;

    public CandlesParameterBool BuildNonTradingCandles;

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

            BuildNonTradingCandles
                = CreateParameterBool("NonTradingCandles", OsLocalization.Market.Label12, false);

            TimeFrame = TimeFrame.Min30;
        }
        else if (state == CandleSeriesState.ParametersChange)
        {

            Enum.TryParse(TimeFrameParameter.ValueString, out TimeFrame newTf);
            TimeFrame = newTf;

            CandlesAll?.Clear();

        }
    }

    public TimeFrame TimeFrame
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                TimeFrameSpan = (TimeSpan)value.GetTimeSpan();
            }
        }
    }

    private TimeSpan TimeFrameSpan { get; set; }

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


        if (lastCandle.Time.Add(TimeFrameSpan + TimeFrameSpan) <= time
                && BuildNonTradingCandles.ValueBool)
        {
            // произошёл пропуск данных в результате клиринга или перерыва в торгах
            SetForeign(time);
        }

        if (lastCandle.Time < time
                && lastCandle.Time.Add(TimeFrameSpan) <= time
                || TimeFrame == TimeFrame.Day
                && lastCandle.Time.Date < time.Date)
        {
            // если пришли данные из новой свечки
            FinishCandle(time, price, volume, canPushUp, side);
            return;
        }

        if (lastCandle.Time.Add(TimeFrameSpan) > time)
        {
            // если пришли данные внутри свечи

            if (lastCandle.State == CandleState.Finished)
            {
                lastCandle.State = CandleState.Started;
            }

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
    }

    private void SetForeign(DateTime now)
    {
        if (CandlesAll.Count == 1) { return; }

        for (int i = 0; i < CandlesAll.Count; i++)
        {
            if (i + 1 < CandlesAll.Count
                    && CandlesAll[i].Time.Add(TimeFrameSpan) < CandlesAll[i + 1].Time
                    || i + 1 == CandlesAll.Count
                    && CandlesAll[i].Time.Add(TimeFrameSpan) < now)
            {
                Candle candle = new()
                {
                    Time = CandlesAll[i].Time.Add(TimeFrameSpan),
                    High = CandlesAll[i].Close,
                    Open = CandlesAll[i].Close,
                    Low = CandlesAll[i].Close,
                    Close = CandlesAll[i].Close,
                    Volume = 1
                };

                CandlesAll.Insert(i + 1, candle);
            }
        }
    }

    protected override void PreUpdateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
    {
        if (TimeFrameSpan.TotalMinutes >= 1)
        {
            if (TimeFrame != TimeFrame.Min45)
            {
                time = time.AddMinutes(-(time.Minute % TimeFrameSpan.TotalMinutes));
            }
            else
            {
                // TODO: Check how it will work
                if (time.Minute < 45)
                {
                    time.AddHours(-1);
                }
                time.AddMinutes(-(time.Minute - 45));
            }
        }

        time = time.AddMilliseconds(-time.Millisecond);
        time = time.AddSeconds(-(time.Second % TimeFrameSpan.TotalSeconds));

        Candle candle = new()
        {
            Time = time,
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

        if (TimeFrameSpan.TotalMinutes >= 1)
        {
            // NOTE: What to do with 45 min?
            if (TimeFrame != TimeFrame.Min45)
            {
                time = time.AddMinutes(-(time.Minute % TimeFrameSpan.TotalMinutes));
            }
        }

        time = time.AddMilliseconds(-time.Millisecond);
        time = time.AddSeconds(-(time.Second % TimeFrameSpan.TotalSeconds));

        // time = CandlesAll[^1].StartTime.AddSeconds((int)TimeFrame);
        Candle newCandle = new()
        {
            Time = time,
            Close = price,
            High = price,
            Low = price,
            Open = price,
            Volume = volume
        };

        CandlesAll.Add(newCandle);

        if (canPushUp)
        {
            UpdateChangeCandle();
        }

    }
}
