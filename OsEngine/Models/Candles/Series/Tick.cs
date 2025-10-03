/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using OsEngine.Language;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles.Series
{
    [Candle("Tick")]
    public class Tick : ACandlesSeriesRealization
    {
        public CandlesParameterInt TradeCount;

        public override void OnStateChange(CandleSeriesState state)
        {
            if (state == CandleSeriesState.Configure)
            {
                TradeCount = CreateParameterInt("TradeCount", OsLocalization.Market.Label11, 1000);
            }
            else if (state == CandleSeriesState.ParametersChange)
            {
                CandlesAll.Clear();
            }
        }

        private int _lastCandleTickCount;

        public override void UpDateCandle(DateTime time, decimal price, decimal volume, bool canPushUp, Side side)
        {
            if (CandlesAll.Count == 0)
            {
                PreUpdateCandle(time, price, volume, canPushUp, side);
                return;
            }

            // если пришли старые данные
            if (CandlesAll[^1].Time > time) { return; }

            if (_lastCandleTickCount >= TradeCount.ValueInt)
            {
                // если пришли данные из новой свечки
                FinishCandle(time, price, volume, canPushUp, side);
                return;
            }

            if (_lastCandleTickCount < TradeCount.ValueInt)
            {
                // если пришли данные внутри свечи
                _lastCandleTickCount++;

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
                Close = price,
                High = price,
                Low = price,
                Open = price,
                Time = time.AddMilliseconds(-time.Millisecond),
                Volume = volume
            };

            CandlesAll.Add(candle);

            if (canPushUp)
            {
                UpdateChangeCandle();
            }

            _lastCandleTickCount = 1;
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
                Close = price,
                High = price,
                Low = price,
                Open = price,
                Time = time.AddMilliseconds(-time.Millisecond),
                Volume = volume
            };

            CandlesAll.Add(newCandle);

            if (canPushUp)
            {
                UpdateChangeCandle();
            }

            _lastCandleTickCount = 1;

        }
    }
}
