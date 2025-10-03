using System;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Color = SkiaSharp.SKColor;

namespace OsEngine.Models.Indicators;

public partial class BaseSeries
{
    // TODO: Add ability create series from operation on serieses?
    // like addition 2 serieses, series * 2 etc

    public string Name
    {
        get;
        init
        {
            field = value;
            ChartSeries.Name = value;
        }
    }

    public ObservableCollection<Point> Values { get; set; } = [];

    internal IStrokedAndFillCartesianSeries<Point> ChartSeries { get; set; } =
        (IStrokedAndFillCartesianSeries<Point>)
        new LineSeries<Point>()
        {
            // So Color can be safely set
            Stroke = new SolidColorPaint(SKColors.Red, 1),
        };

    public required Color Color
    {
        get => ChartSeries.Stroke.Color;
        set => ChartSeries.Stroke.Color = value;
    }

    public float Thickness
    {
        get => ChartSeries.Stroke.StrokeThickness;
        set => ChartSeries.Stroke.StrokeThickness = value;
    }

    public bool IsVisible
    {
        get => ChartSeries.IsVisible;
        set => ChartSeries.IsVisible = value;
    }

    public required ChartSeriesType ChartSeriesType
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            Type type = ChartSeriesType switch
            {
                ChartSeriesType.Line => typeof(LineSeries<Point>),
                ChartSeriesType.Column => typeof(ColumnSeries<Point>),
                ChartSeriesType.Scatter => typeof(ScatterSeries<Point>),
                ChartSeriesType.StepLine => typeof(StepLineSeries<Point>),
                _ => throw new NotImplementedException(),
            };

            var series = (IStrokedAndFillCartesianSeries<Point>)Activator.CreateInstance(type);

            series.Name = Name;
            series.Values = Values;
            series.Stroke = ChartSeries.Stroke;
            series.IsVisible = IsVisible;
            if (series is IEnableNullSplitting s) { s.EnableNullSplitting = false; }

            if (series is ILineSeries line)
            {
                line.LineSmoothness = 0; 
                line.GeometrySize = 0;
            }

            ChartSeries = series;
            ChartTypeChanged?.Invoke(ChartSeries);
            // FIX: need to delete previous series from chart and add this one
        }
    }

    // TODO: Replace 0 with null
    public decimal this[int i]
    {
        get => Values[^(i + 1)]?.Value ?? 0;

        protected internal set
        {
            if (Values[^(i + 1)] == null)
            {
                // NOTE: should be DateTime as well
                // but for this Candle list is needed to get it
                Values[^(i + 1)] = new(value);
            }
            Values[^(i + 1)].Value = value;
        }

        // if (i < 0 || i >= Values.Count)
        //     return null;
        // if (i < 0 || i >= Values.Count)
        //     return;
    }

    // public IPoint this[int i]
    // {
    //     get => Values[^(i + 1)];
    //
    //     protected internal set
    //     {
    //         if (value == null)
    //         {
    //             Values[^(i + 1)].SetNull();
    //             return;
    //         }
    //         Values[^(i + 1)].Value = value;
    //     }
    //
    //     // if (i < 0 || i >= Values.Count)
    //     //     return null;
    //     // if (i < 0 || i >= Values.Count)
    //     //     return;
    // }

    public bool IsNull(int index) => Values[^(index + 1)].IsNull;
    public void SetNull(int index) => Values[^(index + 1)].SetNull();

    public int Count => Values.Count;

    public bool IsEmpty => Values.Count == 0;

    public BaseSeries(string name) => Name = name;

    // public DataSeries(string name, Color color, ChartSeriesType chartSeriesType, bool isVisible)
    // {
    //     Name = name;
    //     IsVisible = isVisible;
    //     Color = color;
    //     ChartSeriesType = chartSeriesType;
    // }

    public BaseSeries(Color color, string name, IndicatorChartPaintType chartSeriesType, bool isPaint)
    {
        Name = name;
        ChartPaintType = chartSeriesType;
        ChartSeriesType = Enum.Parse<ChartSeriesType>(chartSeriesType.ToString());
        Color = color;
        IsVisible = isPaint;
    }

    internal void Add(DateTime dateTime, decimal? value)
    {
        Values.Add(new(dateTime, value));
    }

    internal void Add(DateTime dateTime)
    {
        Values.Add(new(dateTime));
    }

    internal void Clear() => Values.Clear();

    internal void Delete() => Values.Clear();

    internal event Action<LiveChartsCore.ISeries<Point>> ChartTypeChanged;
}

// public abstract partial class BaseSeries2<T>
// {
//     // TODO: Add ability create series from operation on serieses?
//     // like addition 2 serieses, series * 2 etc
//
//     public string Name
//     {
//         get;
//         init
//         {
//             field = value;
//             ChartSeries.Name = value;
//         }
//     }
//
//     public ObservableCollection<Point<T>> Values { get; set; } = [];
//
//     internal IStrokedAndFillCartesianSeries<Point<T>> ChartSeries { get; set; } =
//         (IStrokedAndFillCartesianSeries<Point<T>>)
//         new LineSeries<Point>()
//         {
//             // So Color can be safely set
//             Stroke = new SolidColorPaint(SKColors.Empty, 1),
//         };
//
//     public required Color Color
//     {
//         get => ChartSeries.Stroke.Color;
//         set => ChartSeries.Stroke.Color = value;
//     }
//
//     public float Thickness
//     {
//         get => ChartSeries.Stroke.StrokeThickness;
//         set => ChartSeries.Stroke.StrokeThickness = value;
//     }
//
//     public bool IsVisible
//     {
//         get => ChartSeries.IsVisible;
//         set => ChartSeries.IsVisible = value;
//     }
//
//     public required ChartSeriesType ChartSeriesType
//     {
//         get;
//         set
//         {
//             if (field == value) return;
//             field = value;
//
//             Type type = ChartSeriesType switch
//             {
//                 ChartSeriesType.Line => typeof(LineSeries<Point<T>>),
//                 ChartSeriesType.Column => typeof(ColumnSeries<Point<T>>),
//                 ChartSeriesType.Scatter => typeof(ScatterSeries<Point<T>>),
//                 ChartSeriesType.StepLine => typeof(StepLineSeries<Point<T>>),
//                 _ => throw new NotImplementedException(),
//             };
//
//             var series = (IStrokedAndFillCartesianSeries<Point<T>>)Activator.CreateInstance(type);
//
//             series.Name = Name;
//             series.Values = Values;
//             series.Stroke = ChartSeries.Stroke;
//             series.IsVisible = IsVisible;
//             if (series is IEnableNullSplitting s) { s.EnableNullSplitting = false; }
//
//             if (series is ILineSeries line)
//             {
//                 line.LineSmoothness = 0; 
//                 line.GeometrySize = 0;
//             }
//
//             ChartSeries = series;
//             ChartTypeChanged?.Invoke(ChartSeries);
//             // FIX: need to delete previous series from chart and add this one
//         }
//     }
//
//     // TODO: Replace 0 with null
//     public T this[int i]
//     {
//         get => Values[^(i + 1)];
//
//         protected internal set => Values[^(i + 1)].Value = value;
//
//         // if (i < 0 || i >= Values.Count)
//         //     return null;
//         // if (i < 0 || i >= Values.Count)
//         //     return;
//     }
//
//
//     public int Count => Values.Count;
//
//     public bool IsEmpty => Values.Count == 0;
//
//     public BaseSeries2(string name) => Name = name;
//
//     // public DataSeries(string name, Color color, ChartSeriesType chartSeriesType, bool isVisible)
//     // {
//     //     Name = name;
//     //     IsVisible = isVisible;
//     //     Color = color;
//     //     ChartSeriesType = chartSeriesType;
//     // }
//
//     public BaseSeries2(Color color, string name, IndicatorChartPaintType chartSeriesType, bool isPaint)
//     {
//         Name = name;
//         // ChartPaintType = chartSeriesType;
//         ChartSeriesType = Enum.Parse<ChartSeriesType>(chartSeriesType.ToString());
//         Color = color;
//         IsVisible = isPaint;
//     }
//
//     internal void Add(DateTime dateTime, T value)
//     {
//         Values.Add(new(dateTime, value));
//     }
//
//     internal void Clear() => Values.Clear();
//
//     internal void Delete() => Values.Clear();
//
//     internal event Action<LiveChartsCore.ISeries<Point<T>>> ChartTypeChanged;
// }
//
// public class NullableSeries(string name) : BaseSeries2<decimal?>(name)
// {
// }
//
// public class Series(string name) : BaseSeries2<decimal>(name)
// {
//
// }


internal interface IStrokedAndFillCartesianSeries<T> : IStrokedAndFilled, ICartesianSeries, LiveChartsCore.ISeries<T>
{
    new SolidColorPaint Stroke { get; set; }
}

file interface IEnableNullSplitting
{
    bool EnableNullSplitting { get; set; }
}
