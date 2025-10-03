// #nullable enable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore.Kernel;
using Newtonsoft.Json;

namespace OsEngine.Models.Indicators;

public abstract class IPoint
{
    // private static readonly IPoint _point = new Point(0);

    public abstract decimal Value { get; set; }
    public abstract DateTime DateTime { get; set; }
    public abstract bool IsNull { get; }
    internal abstract void SetNull();

    public static implicit operator decimal(IPoint p) => p.Value;

    // public static implicit operator IPoint(decimal d)
    // {
    //     _point._value = d;
    //     return _point;
    // }
}

public class Point : IPoint, IChartEntity, INotifyPropertyChanged
{
    private decimal _value = 0;

    [JsonIgnore]
    public Coordinate Coordinate { get; set; } = Coordinate.Empty;

    public override decimal Value
    {
        get => _value;
        set
        {
            if (_value == value) { return; }
            _value = value;
            OnPropertyChanged();
        }
    }

    public override DateTime DateTime { get; set; }

    [JsonIgnore]
    public ChartEntityMetaData? MetaData { get; set; }

    public Point(DateTime dateTime, decimal? value)
    {
        DateTime = dateTime;
        if (value.HasValue)
        {
            Value = (decimal)value;
        }
    }

    internal Point(decimal? value) => _value = (decimal)value;

    public Point(DateTime dateTime) => DateTime = dateTime;

    public override bool IsNull => Coordinate.IsEmpty;

    internal override void SetNull()
    {
        if (Coordinate.IsEmpty) { return; }
        _value = 0;
        Coordinate = Coordinate.Empty;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        Coordinate = new(DateTime.Ticks, Convert.ToDouble(Value));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Point<T> : IChartEntity, INotifyPropertyChanged
{
    public T Value
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public DateTime DateTime
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    [JsonIgnore]
    public ChartEntityMetaData? MetaData { get; set; }

    [JsonIgnore]
    public Coordinate Coordinate { get; set; } = Coordinate.Empty;

    public Point(DateTime dateTime, T value)
    {
        DateTime = dateTime;
        Value = value;
        Coordinate = value is null ? Coordinate.Empty : new(dateTime.Ticks, Convert.ToDouble(value));
    }

    public static implicit operator T(Point<T> p) => p.Value;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        Coordinate = Value is null ? Coordinate.Empty : new(DateTime.Ticks, Convert.ToDouble(Value));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
