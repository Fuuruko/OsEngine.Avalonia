/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using OsEngine.Language;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Candles.Series;

public class Reverse : ACandlesSeriesRealization
{
    public CandlesParameterString ValueType;

    public CandlesParameterDecimal ReversCandlesPointsMinMove;

    public CandlesParameterDecimal ReversCandlesPointsBackMove;

    public override void OnStateChange(CandleSeriesState state)
    {
        if (state == CandleSeriesState.Configure)
        {
            ValueType
                = CreateParameterStringCollection("valueType", OsLocalization.Market.Label122,
                "Percent", ["Absolute", "Percent"]);

            ReversCandlesPointsMinMove = CreateParameterDecimal("MinMove", OsLocalization.Market.Label18, 0.2m);
            ReversCandlesPointsBackMove = CreateParameterDecimal("BackMove", OsLocalization.Market.Label19, 0.1m);
        }
        else if (state == CandleSeriesState.ParametersChange)
        {
            CandlesAll?.Clear();
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

        bool candleReady = false;

        Candle lastCandle = CandlesAll[^1];

        // NOTE: Can be simplified by GPT
        if (ValueType.ValueString == "Absolute")
        {
            if (lastCandle.High - lastCandle.Open >= ReversCandlesPointsMinMove.ValueDecimal
                &&
                lastCandle.High - lastCandle.Close >= ReversCandlesPointsBackMove.ValueDecimal)
            { // есть откат от хая
                candleReady = true;
            }

            if (lastCandle.Open - lastCandle.Low >= ReversCandlesPointsMinMove.ValueDecimal
                &&
                lastCandle.Close - lastCandle.Low >= ReversCandlesPointsBackMove.ValueDecimal)
            { // есть откат от лоя
                candleReady = true;
            }
        }
        else if (ValueType.ValueString == "Percent")
        {
            if (lastCandle.High - lastCandle.Open > 0
                && lastCandle.High - lastCandle.Close > 0)
            {
                decimal moveUpPercent = (lastCandle.High - lastCandle.Open) / (lastCandle.Open / 100);
                decimal backMoveFromHighPercent = (lastCandle.High - lastCandle.Close) / (lastCandle.Close / 100);

                if (moveUpPercent >= ReversCandlesPointsMinMove.ValueDecimal
                &&
                backMoveFromHighPercent >= ReversCandlesPointsBackMove.ValueDecimal)
                {// есть откат от хая
                    candleReady = true;
                }
            }

            if (lastCandle.Open - lastCandle.Low > 0
                && lastCandle.Close - lastCandle.Low > 0)
            {
                decimal moveDownPercent = (lastCandle.Open - lastCandle.Low) / (lastCandle.Low / 100);

                decimal backMoveFromLowPercent = (lastCandle.Close - lastCandle.Low) / (lastCandle.Low / 100);

                if (moveDownPercent >= ReversCandlesPointsMinMove.ValueDecimal &&
                    backMoveFromLowPercent >= ReversCandlesPointsBackMove.ValueDecimal)
                { // есть откат от лоя
                    candleReady = true;
                }
            }
        }

        if (candleReady)
        {
            // если пришли данные из новой свечки
            FinishCandle(time, price, volume, canPushUp, side);
            return;
        }
        else
        {
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
