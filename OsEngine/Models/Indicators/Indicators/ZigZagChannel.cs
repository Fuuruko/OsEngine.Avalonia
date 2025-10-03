using System;
using System.Collections.Generic;
using System.Linq;
using OsEngine.Models.Entity;
// using OsEngine.Models.Entity;

namespace OsEngine.Models.Indicators.Indicators;

public class ZigZagChannel_indicator : BaseIndicator
{
    [Parameter]
    public readonly Input.Int period = new("Length", 14);
    // public Int period = Parameter.Int("Length", 14);

    public BaseSeries zigZag = new("ZigZag")
    {
        Color = Colors.CornflowerBlue,
        ChartSeriesType = ChartSeriesType.Point,
        // CanReBuildHistoricalValues = false,
        IsVisible = false,
    };

    public BaseSeries zigZagLine = new("ZigZagLine")
    {
        Color = Colors.CornflowerBlue,
        ChartSeriesType = ChartSeriesType.Point,
        // CanReBuildHistoricalValues = true,
    };

    public BaseSeries zigZagHighs = new("ZigZagHighs")
    {
        Color = Colors.GreenYellow,
        ChartSeriesType = ChartSeriesType.Point,
        // CanReBuildHistoricalValues = true,
        IsVisible = false,
    };

    public BaseSeries zigZagLows = new("ZigZagLows")
    {
        Color = Colors.Red,
        ChartSeriesType = ChartSeriesType.Point,
        // CanReBuildHistoricalValues = true,
    };

    public BaseSeries zigZagUpChannel = new("ZigZagUpChannel")
    {
        Color = Colors.DarkRed,
        ChartSeriesType = ChartSeriesType.Point,
        // CanReBuildHistoricalValues = true,
    };

    public BaseSeries zigZagDownChannel = new("ZigZagDownChannel")
    {
        Color = Colors.DarkGreen,
        ChartSeriesType = ChartSeriesType.Point,
        // CanReBuildHistoricalValues = true,
    };

    public override void OnStateChange(IndicatorState state)
    {
        // _period = CreateParameterInt("Length", 14);
        //
        // _seriesZigZag = CreateSeries("ZigZag", Color.CornflowerBlue, IndicatorChartPaintType.Point, false);
        // _seriesZigZag.CanReBuildHistoricalValues = true;
        //
        // _seriesToLine = CreateSeries("ZigZagLine", Color.CornflowerBlue, IndicatorChartPaintType.Point, true);
        // _seriesToLine.CanReBuildHistoricalValues = true;
        //
        // _seriesZigZagHighs = CreateSeries("_seriesZigZagHighs", Color.GreenYellow, IndicatorChartPaintType.Point, false);
        // _seriesZigZagHighs.CanReBuildHistoricalValues = true;
        //
        // _seriesZigZagLows = CreateSeries("_seriesZigZagLows", Colors.Red, IndicatorChartPaintType.Point, false);
        // _seriesZigZagLows.CanReBuildHistoricalValues = true;
        //
        // _seriesZigZagUpChannel = CreateSeries("_seriesZigZagUpChannel", Color.DarkRed, IndicatorChartPaintType.Point, true);
        // _seriesZigZagUpChannel.CanReBuildHistoricalValues = true;
        //
        // _seriesZigZagDownChannel = CreateSeries("_seriesZigZagDownChannel", Color.DarkGreen, IndicatorChartPaintType.Point, true);
        // _seriesZigZagDownChannel.CanReBuildHistoricalValues = true;
    }

    private decimal _currentZigZagHigh = 0;

    private decimal _currentZigZagLow = 0;

    private int _lastSwingIndex = -1;

    private decimal _lastSwingPrice = 0;

    private int _trendDir = 0;

    public override void OnProcess(List<Candle> candles, int index)
    {
        if (index < period.Value * 2)
        {
            _currentZigZagHigh = 0;
            _currentZigZagLow = 0;
            _lastSwingIndex = -1;
            _lastSwingPrice = 0;
            _trendDir = 0;
            return;
        }

        decimal High = candles[index].High;
        decimal Low = candles[index].Low;

        if (_lastSwingPrice == 0)
            _lastSwingPrice = (High + Low) / 2;

        bool isSwingHigh = High == GetExtremum(candles, period, "High", index);
        bool isSwingLow = Low == GetExtremum(candles, period, "Low", index);
        // bool isSwingHigh = High == candles[index ..].Max(c => c.High);
        // bool isSwingLow = Low == candles[index ..].Min(c => c.Low);
        decimal saveValue = 0;
        bool addHigh = false;
        bool addLow = false;
        bool updateHigh = false;
        bool updateLow = false;

        if (!isSwingHigh && !isSwingLow)
        {
            ReBuildChannel(index);
            return;
        }

        if (_trendDir == 1 && isSwingHigh && High >= _lastSwingPrice)
        {
            saveValue = High;
            updateHigh = true;
        }
        else if (_trendDir == -1 && isSwingLow && Low <= _lastSwingPrice)
        {
            saveValue = Low;
            updateLow = true;
        }
        else if (_trendDir <= 0 && isSwingHigh)
        {
            saveValue = High;
            addHigh = true;
            _trendDir = 1;
        }
        else if (_trendDir >= 0 && isSwingLow)
        {
            saveValue = Low;
            addLow = true;
            _trendDir = -1;
        }

        if (addHigh || addLow || updateHigh || updateLow)
        {
            if (updateHigh && _lastSwingIndex >= 0)
            {
                // zigZag[_lastSwingIndex] = 0; // тут в оригинале double.NaN
                // zigZagHighs[_lastSwingIndex] = 0;
                zigZag.SetNull(_lastSwingIndex);
                zigZagHighs.SetNull(_lastSwingIndex);
                // zigZag[_lastSwingIndex] = null;
                // zigZagHighs[_lastSwingIndex] = null;
            }
            else if (updateLow && _lastSwingIndex >= 0)
            {
                // zigZag[_lastSwingIndex] = 0; // тут в оригинале double.NaN
                // zigZagLows[_lastSwingIndex] = 0;
                // zigZag[_lastSwingIndex] = null;
                // zigZagLows[_lastSwingIndex] = null;
                zigZag.SetNull(_lastSwingIndex);
                zigZagLows.SetNull(_lastSwingIndex);
            }

            if (addHigh || updateHigh)
            {
                _currentZigZagHigh = saveValue;
                // zigZag[index] = _currentZigZagHigh;
                // zigZagHighs[index] = _currentZigZagHigh;
                zigZag[index] = _currentZigZagHigh;
                zigZagHighs[index] = _currentZigZagHigh;

            }
            else if (addLow || updateLow)
            {
                _currentZigZagLow = saveValue;
                // zigZag[index] = _currentZigZagLow;
                // zigZagLows[index] = _currentZigZagLow;
                zigZag[index] = _currentZigZagHigh;
                zigZagLows[index] = _currentZigZagHigh;

            }

            _lastSwingIndex = index;
            _lastSwingPrice = saveValue;

            if (updateHigh || updateLow)
            {
                ReBuildLine(zigZag, zigZagLine);
            }
        }
        ReBuildChannel(index);
    }

    private decimal GetExtremum(List<Candle> candles, int period, string points, int index)
    {
        try
        {
            List<decimal> values = [];
            for (int i = index; i >= index - period; i--)
                values.Add(candles[i].GetPoint(points));

            if (points == "High")
                return values.Max();
            if (points == "Low")
                return values.Min();

        }
        catch (Exception e)
        {

        }

        return 0;
    }

    private void ReBuildChannel(int index)
    {
        // найти три последних максимума 
        List<int> _ZigZagHighs = [];
        // найти три последних минимума 
        List<int> _ZigZagLows = new(3);

        for (int i = index; i >= 0; i--)
        {
            if (zigZagHighs.IsNull(i)) { continue; }
            _ZigZagHighs.Add(i);
            if (_ZigZagHighs.Count == 3) { break; }
        }


        for (int i = index; i >= 0; i--)
        {
            if (zigZagLows.IsNull(i)) { continue; }
            _ZigZagLows.Add(i);
            if (_ZigZagLows.Count == 3) { break; }
        }

        if (_ZigZagHighs.Count < 3 || _ZigZagLows.Count < 3) { return; }

        if (_ZigZagHighs[0] > _ZigZagLows[0])
        {
            // на максимуме
            // UpChannel - предпоследние два экстремума
            RedRawVectorLine(zigZagUpChannel, zigZagHighs, _ZigZagHighs[2], _ZigZagHighs[1], index);

            // DownCannel - последние два экстремума
            RedRawVectorLine(zigZagDownChannel, zigZagLows, _ZigZagLows[1], _ZigZagLows[0], index);
        }
        else
        {
            // на минимуме
            // UpCannel - последние два экстремума
            RedRawVectorLine(zigZagUpChannel, zigZagHighs, _ZigZagHighs[1], _ZigZagHighs[0], index);
            // DownChannel - предпоследние два экстремума
            RedRawVectorLine(zigZagDownChannel, zigZagLows, _ZigZagLows[2], _ZigZagLows[1], index);
        }
    }

    private void RedRawVectorLine(BaseSeries targetSeries, BaseSeries sourceSeries, int _startVectorIndex, int _directionVectorIndex, int _endPointIndex)
    {
        decimal _increment = (sourceSeries[_directionVectorIndex] - sourceSeries[_startVectorIndex]) / (_directionVectorIndex - _startVectorIndex);
        targetSeries[_startVectorIndex] = sourceSeries[_startVectorIndex];
        for (int i = _startVectorIndex + 1; i <= _endPointIndex; i++)
        {
            targetSeries[i] = targetSeries[i - 1] + _increment;
        }
    }
    private void ReBuildLine(BaseSeries zigZag, BaseSeries line)
    {
        decimal curPoint = 0;
        int lastPointIndex = 0;

        for (int i = 0; i < zigZag.Count; i++)
        {
            if (zigZag.IsNull(i)) { continue; }

            if (curPoint == 0)
            {
                curPoint = zigZag[i];
                lastPointIndex = i;
                continue;
            }

            decimal mult = Math.Abs(curPoint - zigZag[i]) / (i - lastPointIndex);

            if (zigZag[i] < curPoint)
            {
                mult *= -1;
            }

            decimal curValue = curPoint;

            for (int i2 = lastPointIndex; i2 < i; i2++)
            {
                line[i2] = curValue;
                curValue += mult;
            }

            curPoint = zigZag[i];
            lastPointIndex = i;
        }
    }
}
