using System;
using System.Linq;
using Newtonsoft.Json;

namespace OsEngine.Models.Indicators;

public partial class BaseSeries
{
    // NOTE: Dont understood what it do
    [JsonIgnore]
    [Obsolete]
    public bool CanReBuildHistoricalValues;

    /// <summary>
    /// graph type for data series. Line, column, etc...
    /// </summary>
    [JsonIgnore]
    [Obsolete(nameof(ChartSeriesType))]
    public IndicatorChartPaintType ChartPaintType {
        get;
        set
        {
            field = value;
            ChartSeriesType = Enum.Parse<ChartSeriesType>(value.ToString());
        }
    }

    /// <summary>
    /// whether this series of data needs to be plotted on a chart
    /// </summary>
    [JsonIgnore]
    [Obsolete(nameof(IsVisible))]
    public bool IsPaint
    {
        get => IsVisible;
        set => IsVisible = value;
    }

    /// <summary>
    /// the last value of the series
    /// </summary>
    [JsonIgnore]
    [Obsolete("Use [0] instead")]
    public decimal Last => Values.LastOrDefault()?.Value ?? 0;
}
