using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using OsEngine.Models.Entity;
using OsEngine.Models.Indicators;

namespace OsEngine.ViewModels.Terminal;

public partial class ChartViewModel : BaseViewModel
{
    public string Name { get; }

    public ICartesianAxis[] XAxes { get; set; }
    public ICartesianAxis[] YAxes { get; set; }

    public ChartArea MainArea { get; } = new()
    {
        Num = 0,
    };

    public ObservableCollection<Candle> _candles = [];
    public CandlesticksSeries<Candle> Candles = new();

    public ObservableCollection<ChartArea> ChartAreas { get; } = [];

    public ChartViewModel()
    {
        ChartAreas.Add(MainArea);
        Candles.Values = _candles;
        MainArea.Series.Add(Candles);
        MainArea.XAxes = XAxes;
        MainArea.YAxes = YAxes;

        _candles.Add(new(){ Time=DateTime.Now, Low=1, Close=2, Open=3, High=4 });
        _candles.Add(new(){ Time=DateTime.Now.AddDays(1), Low=1, Close=3, Open=2, High=4 });
        _candles.Add(new(){ Time=DateTime.Now.AddDays(2), Low=1, Close=3, Open=2, High=4 });
        _candles.Add(new(){ Time=DateTime.Now.AddDays(4), Low=1, Close=3, Open=2, High=4 });

        var candlesAxis = new DateTimeAxis(TimeSpan.FromDays(1),
                dt => dt.ToString("yyyy MM dd"))
        {
            LabelsRotation = 15,
            ShowSeparatorLines = true,
            TicksAtCenter = true,
            CrosshairLabelsPaint = new SolidColorPaint(Colors.AliceBlue),
            CrosshairPaint = new SolidColorPaint(Colors.AliceBlue, 1),
        };
        XAxes = [
            // new DateTimeAxis(TimeSpan.FromDays(1), dt => dt.ToString("yyyy MM dd"))
            // {
            //     LabelsRotation = 15,
            //     Labeler = Labelers.Default,
            //     // Labels = _candles
            //     //     .Select(x => x.StartTime.ToString("yyyy MMM dd"))
            //     //     .ToArray(),
            // },
            candlesAxis,
        ];
        YAxes = [
            new Axis()
            {
                Labeler = num => num.ToString("0.00000"),
                CrosshairLabelsPaint = new SolidColorPaint(Colors.AliceBlue),
                CrosshairPaint = new SolidColorPaint(Colors.AliceBlue, 1),
            }
        ];
        // SharedAxes.Set(XAxes[0], XAxes[1]);


        Random random = new();

        // Generate an array of n random numbers in one line using LINQ
        List<DateTimePoint> r = [.. _candles.Select(c => new DateTimePoint(c.Time, random.Next(1, 5)))];
        r.Insert(3, null);
        LineSeries<DateTimePoint> s = new()
        {
            EnableNullSplitting = false,
            Values = r,
            LineSmoothness = 0,
            GeometrySize = 5,
            Fill = null,
            // ScalesXAt = 1
        };
        ChartAreas.Add(new()
                {
                XAxes = XAxes,
                YAxes = YAxes,
                }
                );
        ChartAreas[^1].Series.Add(s);
        MainArea.Series.Add(s);
        // Areas.Series.Add(s);
    }

    public void AddIndicatorCommand(IIndicator indicator)
    {
        DateTime t = DateTime.Now;
        decimal h = 8;
        decimal c = 7;
        decimal o = 6;
        decimal l = 5;
        new Coordinate(t.Ticks, (double)h, (double)c, (double)o , (double)l);
    }

    public void DeleteIndicatorCommand()
    {

    }
}

public class ChartArea
{
    public int Num { get; set; }

    public ObservableCollection<ISeries> Series { get; } = [];

    public ICartesianAxis[] XAxes { get; set; }
    public ICartesianAxis[] YAxes { get; set; }
    public float Height { get; set; } = 100;
}
