/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace OsEngine.Models.Entity;

public partial class Candle : ObservableObject, IChartEntity, ICandle
{
    // NOTE: Maybe just Time?
    [ObservableProperty]
    private DateTime time;

    // NOTE: If Close will change High/Low then
    // there should be default value Max/Min for Low/High
    [ObservableProperty]
    private decimal open;

    [ObservableProperty]
    private decimal high;

    // NOTE: May if change close also check
    // if it more/less that high/low and change them as well?
    [ObservableProperty]
    private decimal close;
    // public decimal Close
    // {
    //     get;
    //     set
    //     {
    //         if (value > high)
    //             high = value;
    //         else if (value < low)
    //             low = value;
    //         field = value;
    //         OnPropertyChanged();
    //     }
    // }

    [ObservableProperty]
    private decimal low;

    // NOTE: Should call base.OnPropertyChanged
    [ObservableProperty]
    private decimal volume;

    // NOTE: Not used anywhere except transaq
    public decimal OpenInterest { get; set; }

    public decimal Median => (High + Low) / 2m;

    public decimal Typical => (High + Low + Close) / 3m;

    public decimal this[string type] => ((ICandle)this)[type];

    /// <summary>
    /// Candles completion status
    /// </summary>
    // NOTE: Probably can be removed?
    // Maybe create my own array that contain List<Candle>
    // and List<Trade>. candles.Trades[i], candles[i] => return candle;
    // or maybe dont change anything cause memeory save is insignificant
    public CandleState State { get; set; } = CandleState.Started;
    public bool IsFinished => State == CandleState.Finished;
    public bool IsActive => State == CandleState.Started;

    /// <summary>
    /// The trades that make up this candle
    /// </summary>
    public List<Trade> Trades { get; } = [];
    IReadOnlyCollection<Trade> ICandle.Trades => Trades;

    public Coordinate Coordinate { get; set; }

    public ChartEntityMetaData MetaData { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        // NOTE: TimeStart maybe change to somethings else
        Coordinate = new Coordinate(Time.Ticks, (double)High, (double)Open, (double)Close, (double)Low);
        base.OnPropertyChanged(e);
    }

    internal void _Parse(string s)
    {
        //20131001,100000,97.8000000,97.9900000,97.7500000,97.9000000,1
        //<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOLUME>,<OPEN INTEREST>
        string[] sIn = s.Split(',');

        Time = DateTimeParseHelper.ParseFromTwoStrings(sIn[0], sIn[1]);

        Open = sIn[2].ToDecimal();
        High = sIn[3].ToDecimal();
        Low = sIn[4].ToDecimal();
        Close = sIn[5].ToDecimal();

        try
        {
            Volume = sIn[6].ToDecimal();
        }
        catch (Exception)
        {
            Volume = 1;
        }

        if(sIn.Length > 7)
        {
            try
            {
                OpenInterest = sIn[7].ToDecimal();
            }
            catch
            {
                // ignore
            }
        }
    }

    internal static Candle Parse(string s)
    {
        //20131001,100000,97.8000000,97.9900000,97.7500000,97.9000000,1
        //<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOLUME>,<OPEN INTEREST>
        string[] sIn = s.Split(',');
        Candle candle = new()
        {
            Time = DateTimeParseHelper.ParseFromTwoStrings(sIn[0], sIn[1]),
            Open = sIn[2].ToDecimal(),
            High = sIn[3].ToDecimal(),
            Low = sIn[4].ToDecimal(),
            Close = sIn[5].ToDecimal()
        };

        try
        {
            candle.Volume = sIn[6].ToDecimal();
        }
        catch (Exception)
        {
            candle.Volume = 1;
        }

        if(sIn.Length > 7)
        {
            try
            {
                candle.OpenInterest = sIn[7].ToDecimal();
            }
            catch
            {
                // ignore
            }
        }
        return candle;
    }
    /// <summary>
    /// Take a line of signatures
    /// </summary>
    // internal string ToolTip
    // {
    //     //Date - 20131001 Time - 100000 
    //     // Open - 97.8000000 High - 97.9900000 Low - 97.7500000 Close - 97.9000000 Body(%) - 0.97
    //     get
    //     {
    //
    //         string result = string.Empty;
    //
    //         if (TimeStart.Day > 9)
    //         {
    //             result += TimeStart.Day.ToString();
    //         }
    //         else
    //         {
    //             result += "0" + TimeStart.Day;
    //         }
    //
    //         result += ".";
    //
    //         if (TimeStart.Month > 9)
    //         {
    //             result += TimeStart.Month.ToString();
    //         }
    //         else
    //         {
    //             result += "0" + TimeStart.Month;
    //         }
    //
    //         result += ".";
    //         result += TimeStart.Year.ToString();
    //
    //         result += " ";
    //
    //         if (TimeStart.Hour > 9)
    //         {
    //             result += TimeStart.Hour.ToString();
    //         }
    //         else
    //         {
    //             result += "0" + TimeStart.Hour;
    //         }
    //
    //         result += ":";
    //
    //         if (TimeStart.Minute > 9)
    //         {
    //             result += TimeStart.Minute.ToString();
    //         }
    //         else
    //         {
    //             result += "0" + TimeStart.Minute;
    //         }
    //
    //         result += ":";
    //
    //         if (TimeStart.Second > 9)
    //         {
    //             result += TimeStart.Second.ToString();
    //         }
    //         else
    //         {
    //             result += "0" + TimeStart.Second;
    //         }
    //
    //         result += "  \r\n";
    //
    //         result += " O: ";
    //         result += Open.ToStringWithNoEndZero();
    //         result += " H: ";
    //         result += High.ToStringWithNoEndZero();
    //         result += " L: ";
    //         result += Low.ToStringWithNoEndZero();
    //         result += " C: ";
    //         result += Close.ToStringWithNoEndZero();
    //
    //         result += "  \r\n";
    //
    //         result += " Body(%): ";
    //         result += (Math.Floor(BodyPercent * 100m) / 100m).ToStringWithNoEndZero();
    //
    //         return result;
    //     }
    // }

    private string _stringToSave;
    private decimal _closeWhenGotLastString;
    // NOTE: ??? Like in what case?
    [Obsolete($"Use {nameof(ToString)} instead")]
    internal string StringToSave
    {
        get
        {
            if (_closeWhenGotLastString == Close)
            {
                // If we've taken candles before, we're not counting on that line.
                return _stringToSave;
            }

            _closeWhenGotLastString = Close;

            _stringToSave = "";

            //20131001,100000,97.8000000,97.9900000,97.7500000,97.9000000,1,0.97
            //<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOLUME>,<OPEN INTEREST>

            string result = "";
            result += Time.ToString("yyyyMMdd,HHmmss") + ",";

            result += Open.ToString(CultureInfo.InvariantCulture) + ",";
            result += High.ToString(CultureInfo.InvariantCulture) + ",";
            result += Low.ToString(CultureInfo.InvariantCulture) + ",";
            result += Close.ToString(CultureInfo.InvariantCulture) + ",";
            result += Volume.ToString(CultureInfo.InvariantCulture) + ",";
            result += OpenInterest.ToString(CultureInfo.InvariantCulture);

            _stringToSave = result;

            return _stringToSave;
        }
    }

    public override string ToString()
    {
        if (_closeWhenGotLastString == Close)
        {
            // If we've taken candles before, we're not counting on that line.
            return _stringToSave;
        }

        _closeWhenGotLastString = Close;

        _stringToSave = "";

        //20131001,100000,97.8000000,97.9900000,97.7500000,97.9000000,1,0.97
        //<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOLUME>,<OPEN INTEREST>

        string result = "";
        result += Time.ToString("yyyyMMdd,HHmmss") + ",";

        result += Open.ToString(CultureInfo.InvariantCulture) + ",";
        result += High.ToString(CultureInfo.InvariantCulture) + ",";
        result += Low.ToString(CultureInfo.InvariantCulture) + ",";
        result += Close.ToString(CultureInfo.InvariantCulture) + ",";
        result += Volume.ToString(CultureInfo.InvariantCulture) + ",";
        result += OpenInterest.ToString(CultureInfo.InvariantCulture);

        _stringToSave = result;

        return _stringToSave;
    }

    [Obsolete($"Use {nameof(Time)} instead")]
    public DateTime TimeStart { get => Time; set => Time = value; }

    [Obsolete("Use Median instead")]
    public decimal Center => Median;

    /// <summary>
    /// Certain point on the candle
    /// </summary>
    /// <param name="type"> "Close","High","Low","Open","Median","Typical"</param>
    [Obsolete("Use short form Candle[type] instead")]
    public decimal GetPoint(string type) => this[type];

    /// <summary>
    /// To load the status of the candlestick from the line
    /// </summary>
    /// <param name="s">status line</param>
    [Obsolete($"Use {nameof(_Parse)} instead")]
    internal void SetCandleFromString(string s) => _Parse(s);



    /// <summary>
    /// Close > Open
    /// </summary>
    public bool IsUp => Close > Open;

    /// <summary>
    /// Close < Open
    /// </summary>
    public bool IsDown => Close < Open;

}

/// <summary>
/// Candle formation status
/// </summary>
public enum CandleState
{
    /// <summary>
    /// Completed
    /// </summary>
    Finished,

    Started,

    /// <summary>
    /// Indefinitely
    /// </summary>
    None
}

public interface ICandle
{
    DateTime Time { get; }

     decimal Open { get; }

     decimal High { get; }

     decimal Close { get; }

     decimal Low { get; }

     decimal Volume { get; }

    // NOTE: Not used anywhere except transaq
    decimal OpenInterest { get; }

    /// <summary>
    /// (High + Low) / 2
    /// </summary>
    decimal Median { get; }

    /// <summary>
    /// (High + Low + Close) / 3
    /// </summary>
    decimal Typical { get; }

    decimal this[string type]
    {
        get
        {
            return type switch
            {
                "Open" => Open,
                "High" => High,
                "Low" => Low,
                "Close" => Close,
                "Median" => Median,
                "Typical" => Typical,
                _ => throw new Exception($"Wrong string for Candle type: {type}\nAllowed strings: Open, High, Low, Close, Median, Typical"),
            };
        }
    }

    /// <summary>
    /// Open
    /// </summary>
    decimal O => Open;

    /// <summary>
    /// Close
    /// </summary>
    decimal C => Close;

    /// <summary>
    /// High
    /// </summary>
    decimal H => High;

    /// <summary>
    /// Low
    /// </summary>
    decimal L => Low;

    /// <summary>
    /// Volume
    /// </summary>
    decimal V => Volume;

    /// <summary>
    /// Open Interest
    /// </summary>
    decimal I => OpenInterest;

    /// <summary>
    /// Median: (High + Low) / 2
    /// </summary>
    decimal M => Median;

    /// <summary>
    /// Typical : (High + Low + Close) / 3
    /// </summary>
    decimal T => Typical;

    /// <summary>
    /// Candles completion status
    /// </summary>
    CandleState State { get; }

    /// <summary>
    /// The trades that make up this candle
    /// </summary>
    // List<Trade> Trades { set; get; }
    IReadOnlyCollection<Trade> Trades { get; }

    /// <summary>
    /// Close > Open
    /// </summary>
    bool IsUp => Close > Open;

    /// <summary>
    /// Close < Open
    /// </summary>
    bool IsDown => Close < Open;

    /// <summary>
    /// Close == Open
    /// </summary>
    bool IsDoji => Close == Open;

    decimal ShadowTop => High - (IsUp ? Close : Open);

    decimal ShadowBottom => (IsUp ? Open : Close) - Low;

    /// <summary>
    /// Candle body with shadows
    /// </summary>
    decimal ShadowBody => High - Low;

    /// <summary>
    /// Candle body without shadows
    /// </summary>
    decimal Body => IsUp ? Close - Open : Open - Close;

    /// <summary>
    /// Candle body (%)
    /// </summary>
    decimal BodyPercent
    {
        get
        {
            // NOTE: ??? Like in what case?
            if (Close <= 0m || Open <= 0m)
            {
                return 0m;
            }
            return Math.Abs(Close - Open) / Open * 100m;
            if (ShadowBody != 0)
                return Math.Abs(Close - Open) / ShadowBody * 100m;
        }
    }

    /// <summary>
    /// Candle volatility (regarding center, %)
    /// </summary>
    // NOTE: Not sure if it mean something
    decimal Volatility
    {
        get
        {
            // NOTE: ??? Like in what case?
            if (Median == 0m)
            {
                return 0m;
            }
            // NOTE: Eq 100 * (H - L) / (H + L)
            return (High - Median) / Median * 100m;
        }
    }

    [Obsolete($"Use {nameof(Time)} instead")]
    DateTime TimeStart { get; }

    [Obsolete("Use Median instead")]
    decimal Center { get; }
}
