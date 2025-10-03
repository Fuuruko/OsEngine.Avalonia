using System;
using System.Collections.Generic;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Indicators;

public partial class BaseIndicator
{
    private List<Candle> _myCandles = [];

    private Candle _lastFirstCandle = null;

    public void Process(List<Candle> candles)
    {
        //lock(_indicatorUpdateLocker)
        //{
        if (candles.Count == 0 || DataSeries.Count == 0)
        {
            return;
        }

        if (_myCandles == null ||
                candles.Count < _myCandles.Count ||
                candles.Count > _myCandles.Count + 1 ||
                _lastFirstCandle != null && _lastFirstCandle.Time != candles[0].Time)
        {
            ProcessAll(candles);
        }
        else if (candles.Count < DataSeries[0].Count)
        {
            foreach (var ds in DataSeries)
            {
                ds.Clear();
            }
            ProcessAll(candles);
        }
        else if (_myCandles.Count == candles.Count)
        {
            ProcessLast(candles);
        }
        else if (_myCandles.Count + 1 == candles.Count)
        {
            ProcessNew(candles, candles.Count - 1);
        }

        _myCandles = candles;
        _lastFirstCandle = candles[0];
        //}
    }

    private void ProcessAll(List<Candle> candles)
    {
        foreach (var i in IncludeIndicators)
        {
            i.Clear();
            i.Process(candles);
        }

        foreach (var s in DataSeries)
        {
            s.Clear();
        }

        for (int i = 0; i < candles.Count; i++)
        {
            ProcessNew(candles, i);
        }
    }

    private void ProcessLast(List<Candle> candles)
    {
        foreach (BaseSeries series in DataSeries)
        {
            if (series.Count == 0 && candles.Count > 0)
            {
                series.Add(candles[0].Time, null);
            }

            while (series.Count < candles.Count)
            {
                series.Add(candles[series.Count + 1].Time, series[^1]);
            }
        }

        if (candles.Count <= 0) { return; }

        foreach (BaseIndicator indicator in IncludeIndicators)
        {
            indicator.Process(candles);
        }

        if (IsOn) { OnProcess(candles, candles.Count - 1); }
    }

    private void ProcessNew(List<Candle> candles, int index)
    {
        if (candles.Count <= 0 || index < 0)
        {
            return;
        }

        foreach (BaseSeries series in DataSeries)
        {
            if (series.Count < candles.Count
                    && series.Count == 0)
            {
                series.Add(candles[0].Time, 0);
            }
            while (series.Count < candles.Count)
            {
                series.Add(candles[series.Count + 1].Time, series[^1]);
            }
        }

        for (int i = 0; i < IncludeIndicators.Count; i++)
        {
            IncludeIndicators[i].ProcessNew(candles,index);
        }

        if (IsOn) { OnProcess(candles, index); }
    }

    public void Reload()
    {
        if (_myCandles == null) { return; }

        ProcessAll(_myCandles);
        NeedToReloadEvent?.Invoke(this);
    }

    public void RePaint()
    {
        NeedToReloadEvent?.Invoke(this);
    }

    public void OnReloadRequired() => ReloadRequired?.Invoke(this);

    public event Action<BaseIndicator> NeedToReloadEvent;
    public event Action<BaseIndicator> ReloadRequired;
}
