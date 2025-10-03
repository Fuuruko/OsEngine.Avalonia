/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Language;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles.Series
{
    [Candle("TimeShiftCandle")]
    [Name("Timeshift Candle")]
    public class TimeShiftCandle : ACandlesSeriesRealization
    {
        public CandlesParameterString TimeFrameParameter;

        public CandlesParameterInt SecondsShift;

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

                SecondsShift = CreateParameterInt("SecondsShift", OsLocalization.Market.Label123, -3);

                TimeFrame = TimeFrame.Min30;
                CreateCandlesTimes();
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
                try
                {
                    if (value != field)
                    {

                        field = value;
                        TimeFrameSpan = (TimeSpan)value.GetTimeSpan();
                        CreateCandlesTimes();
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        public TimeSpan TimeFrameSpan { get; private set; }

        List<DateTime> _timesStartCandles = [];

        private void CreateCandlesTimes()
        {
            _timesStartCandles = [];

            DateTime firstCandleTime = new(2022, 1, 1, 0, 0, 0);

            _timesStartCandles.Add(firstCandleTime);

            DateTime nextCandleTime = firstCandleTime.Add(TimeFrameSpan);

            // NOTE: Something strange
            while (true)
            {
                if (nextCandleTime.Day != firstCandleTime.Day)
                {
                    break;
                }

                _timesStartCandles.Add(nextCandleTime);
                nextCandleTime = nextCandleTime.Add(TimeFrameSpan);
            }

            for (int i = 0; i < _timesStartCandles.Count; i++)
            {
                _timesStartCandles[i] = _timesStartCandles[i].AddSeconds(SecondsShift.ValueInt);
            }
        }

        public override void UpDateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
        {
            if (CandlesAll.Count == 0)
            {
                // пришла первая сделка
                PreUpdateCandle(time, price, volume, canPushUp, side);
                return;
            }

            // если пришли старые данные
            if (CandlesAll[^1].Time > time) { return; }

            if (CandlesAll[^1].Time.Add(TimeFrameSpan) <= time)
            {
                // если пришли данные из новой свечки
                FinishCandle(time, price, volume, canPushUp, side);
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
            DateTime timeNextCandle = time;

            for (int i = 0; i < _timesStartCandles.Count - 1; i++)
            {
                DateTime curTime = _timesStartCandles[i];
                DateTime nextTime = _timesStartCandles[i + 1];

                if (time.TimeOfDay >= curTime.TimeOfDay &&
                        time.TimeOfDay < nextTime.TimeOfDay)
                {
                    timeNextCandle = curTime;
                    break;
                }
            }

            timeNextCandle = new DateTime(time.Year, time.Month, time.Day,
                    timeNextCandle.Hour, timeNextCandle.Minute, timeNextCandle.Second, timeNextCandle.Millisecond);

            Candle candle = new()
            {
                Close = price,
                High = price,
                Low = price,
                Open = price,
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

            for (int i = 0; i < _timesStartCandles.Count - 1; i++)
            {
                DateTime curTime = _timesStartCandles[i];
                DateTime nextTime = _timesStartCandles[i + 1];

                if (time.TimeOfDay >= curTime.TimeOfDay &&
                        time.TimeOfDay < nextTime.TimeOfDay)
                {
                    timeNextCandle = curTime;
                    break;
                }
            }

            timeNextCandle = new DateTime(time.Year, time.Month, time.Day,
                    timeNextCandle.Hour, timeNextCandle.Minute, timeNextCandle.Second, timeNextCandle.Millisecond);

            Candle newCandle = new()
            {
                Close = price,
                High = price,
                Low = price,
                Open = price,
                Time = timeNextCandle,
                Volume = volume
            };

            CandlesAll.Add(newCandle);

            if (canPushUp)
            {
                UpdateChangeCandle();
            }
        }
    }
}
