/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using OsEngine.Models.Entity;
using Color = SkiaSharp.SKColor;
using SkiaSharp;
using System.Linq;

namespace OsEngine.Models.Indicators;

public abstract partial class BaseIndicator
{
    public abstract void OnStateChange(IndicatorState state);
    public abstract void OnProcess(List<Candle> source, int index);

    // NOTE: Can be depricated?
    public StartProgram StartProgram;

    // public IndicatorChartPaintType TypeIndicator { get; set; }

    public List<BaseSeries> DataSeries = [];
    // public List<IndicatorDataSeries> Series = [];

    public bool CanUserDelete { get; set; }

    public string NameSeries { get; set; }

    public string NameArea { get; set; }

    public string Name { get; set; }

    public List<BaseInput<dynamic>> Parameters { get; } = [];

    public bool IsVisible
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            foreach (var i in IncludeIndicators)
                i.IsVisible = value;

            foreach (var s in DataSeries)
                s.IsVisible = value;
        }
    }

    // NOTE: Turn off indicator for speed when optimizing
    // but robots probably should not have excessive indicators?
    public bool IsOn
    {
        get;
        set
        {
            if (field == value) { return; }

            foreach (var i in IncludeIndicators)
            {
                i.IsOn = value;
            }
        }
    } = true;

    // NOTE: Add checking or preloading
    // Also not sure about set, does it needed?
    public decimal? this[int series, int index] => DataSeries[series][index];

    public decimal? this[int index] => DataSeries[0][index];

    // [Parameter(Color)]
    public void Init(string name, StartProgram startProgram)
    {
        if (name == "") { throw new("Empty name provided"); }
        Name = name;
        CanDelete = true;

        if (startProgram != StartProgram.IsOsOptimizer)
        {
            Load();
        }

        OnStateChange(IndicatorState.Configure);
    }

    public static Color Color(string color)
    {
        var color_ = Avalonia.Media.Color.Parse(color);
        return new(color_.R, color_.G, color_.B, color_.A);
    }

    public static Color Color(byte r, byte g, byte b, byte a) => new(r, g, b, a);

    protected static readonly Colors Colors = new();


    public void ShowDialog()
    {
        // FIX: Fix needed
        // AIndicatorUi ui = new AIndicatorUi(this);
        // ui.ShowDialog();
        //
        // if (ui.IsAccepted)
        // {
        //     Reload();
        //
        //     Save();
        // }
    }



    private void OnParameterChangedByUser() => ParametersChangeByUser?.Invoke();

    public event Action ParametersChangeByUser;

    // protected Parameter Parameter = new();

    public Dictionary<string, BaseIndicator> IncludeIndicatorsD = [];


    public void ProcessIndicator(string indicatorName, BaseIndicator indicator)
    {
        IncludeIndicators.Add(indicator);
        IncludeIndicatorsName.Add(indicatorName);

        IncludeIndicatorsD.Add(indicatorName, indicator);
    }


    internal void Binding()
    {
        FieldInfo[] fields = GetType().GetFields(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                );
        PropertyInfo[] properties = GetType().GetProperties(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                );

        MemberInfo[] members = GetType().GetMembers();
        foreach (var m in members)
        {
            ParameterAttribute attribute = m.GetCustomAttribute<ParameterAttribute>();
            if (attribute != null)
            {
                // Parameters.Add(m.GetCustomAttribute);
            }
        }

        // List<ParameterAttribute> attributes = GetType().GetCustomAttributes<ParameterAttribute>();

        Type baseParameter = typeof(BaseInput<>);
        foreach (var property in properties)
        {
            // Check if the property is of type B or derived from B
            Type propertyType = property.PropertyType;
            if (propertyType.IsGenericType
                    && propertyType.GetGenericTypeDefinition() == baseParameter)
            {
                Parameters.Add((BaseInput<dynamic>)property.GetValue(this));
                // Get the value of the property
                // var value = property.GetValue(this);
                // if (value != null)
                // {
                //     // Add to the list
                //     bList.Add((B)value);
                // }
            }
        }
    }


    public BaseSeries CreateSeries(string name, Color color,
            IndicatorChartPaintType chartPaintType, bool isPaint)
    {
        if (DataSeries.Find(val => val.Name == name) != null)
        {
            return DataSeries.Find(val => val.Name == name);
        }

        BaseSeries newSeries = new(name)
        {
            Color = color,
            ChartSeriesType = Enum.Parse<ChartSeriesType>(chartPaintType.ToString()),
            IsVisible = isPaint,
        };
        DataSeries.Add(newSeries);
        CheckSeriesParametersInSaveData(newSeries);

        return newSeries;
    }


    public virtual bool PreProcess(List<Candle> candles) => true;


    public virtual void OnCandleFinished(Candle candle)
    {

    }

    public virtual void OnCandleChanged(Candle candle)
    {

    }

    public virtual void OnTick(Candle candle)
    {

    }
}

public enum IndicatorState
{
    Configure,
    Dispose,
}

/// <summary>
/// Allow index Indicator directly(e.g. SMA[0])
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MainSeriesAttribute() : Attribute
{
}
