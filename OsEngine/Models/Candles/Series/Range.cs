/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Language;
using OsEngine.Models.Candles.Factory;
using OsEngine.Models.Entity;
using System;

namespace OsEngine.Models.Candles.Series;

[Candle("Range")]
public class Range : ACandlesSeriesRealization
{
    public CandlesParameterString ValueType;

    public CandlesParameterDecimal RangeCandlesPoints;

    public override void OnStateChange(CandleSeriesState state)
    {
        if (state == CandleSeriesState.Configure)
        {
            ValueType
                = CreateParameterStringCollection("valueType", OsLocalization.Market.Label122,
                "Percent", ["Absolute", "Percent"]);

            RangeCandlesPoints = CreateParameterDecimal("MinMove", OsLocalization.Market.Label18, 0.2m);
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
            PreUpdateCandle(time, price, volume, canPushUp, side);
            return;
        }

        // если пришли старые данные
        if (CandlesAll[^1].Time > time) { return; }

        bool isNewCandle = false;

        if (ValueType.ValueString == "Absolute")
        {
            if (CandlesAll[^1].High - CandlesAll[^1].Low >= RangeCandlesPoints.ValueDecimal)
            {
                isNewCandle = true;
            }
        }
        else if (ValueType.ValueString == "Percent")
        {
            decimal distance = CandlesAll[^1].High - CandlesAll[^1].Low;

            decimal movePercent = distance / (CandlesAll[^1].Low / 100);

            if (distance != 0
                && movePercent != 0
                && movePercent > RangeCandlesPoints.ValueDecimal)
            {
                isNewCandle = true;
            }
        }

        if (isNewCandle)
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
