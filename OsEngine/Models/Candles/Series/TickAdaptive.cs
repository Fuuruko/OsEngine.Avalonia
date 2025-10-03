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

[Candle("TickAdaptive")]
[Name("Tick Adaptive")]
public class TickAdaptive : ACandlesSeriesRealization
{
    public CandlesParameterInt TradeCount;

    public CandlesParameterInt CandlesCountInDay;

    public CandlesParameterInt DaysLookBack;

    public override void OnStateChange(CandleSeriesState state)
    {
        if (state == CandleSeriesState.Configure)
        {
            TradeCount = CreateParameterInt("TradeCount", OsLocalization.Market.Label11, 1000);
            CandlesCountInDay = CreateParameterInt("CandlesCountInDay", OsLocalization.Market.Label125, 100);
            DaysLookBack = CreateParameterInt("DaysLookBack", OsLocalization.Market.Label126, 1);
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
            if (TradeCount.ValueInt <= 0)
            {
                TradeCount.ValueInt = 1;
            }
        }
    }

    private void RebuildCandlesCount()
    {
        int daysLookBack = DaysLookBack.ValueInt;

        if (CandlesAll[^1].Trades.Count == 0
                && daysLookBack > 2)
        {
            daysLookBack = 2;
        }

        decimal candlesCount = 0;

        DateTime date = CandlesAll[^1].Time.Date;

        int days = 0;

        int tradesCount = 0;

        int i;
        for (i = CandlesAll.Count - 1; i >= 0; i--)
        {
            Candle curCandle = CandlesAll[i];

            if (curCandle.Time.Date < date)
            {
                date = curCandle.Time.Date;
                days++;
            }

            if (days >= daysLookBack) { break; }

            tradesCount += curCandle.Trades.Count;

            candlesCount++;

        }

        if (i == 0) { days++; }

        if (candlesCount == 0) { return; }

        if (tradesCount == 0)
        {
            decimal countCandlesInDay = candlesCount / days;
            decimal commonTradesCount = TradeCount.ValueInt * countCandlesInDay;
            decimal newTradesCount = commonTradesCount / CandlesCountInDay.ValueInt;
            TradeCount.ValueInt = Convert.ToInt32(newTradesCount);
        }
        else
        {
            decimal newTradesCount = tradesCount / days / CandlesCountInDay.ValueInt;
            TradeCount.ValueInt = Convert.ToInt32(newTradesCount);
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

        if (CandlesAll[^1].Time.Date < time.Date)
        {
            // пришли данные из нового дня

            RebuildCandlesCount();
            FinishCandle(time, price, volume, canPushUp, side);
            return;
        }

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

        _lastCandleTickCount = 1;
    }
}
