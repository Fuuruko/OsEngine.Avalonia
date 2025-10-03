using System;
using System.Globalization;
using OsEngine.Models.Logging;
using OsEngine.Models.Market;

namespace OsEngine.ViewModels.Data;

public partial class SetViewModel : BaseViewModel, ILog
{
    public string Name { get; set; }
    public bool IsEnabled { get; set; }

    public SecuritySettings Settings { get; set; }

    public event Action<string, LogMessageType> LogRecieved;

    public void AddNewSecurity()
    {

    }

    public void DeleteSecurity()
    {

    }

    public void OnLogRecieved(string message, LogMessageType type)
    {
        if (LogRecieved != null)
        {
            LogRecieved(message, type);
        }
        else
        {
            MessageBox.Show(message);
        }
    }
}

public class SecuritySettings
{
    public SecuritySettings()
    {
        Tf1MinuteIsOn = false;
        Tf2MinuteIsOn = false;
        Tf5MinuteIsOn = true;
        Tf10MinuteIsOn = false;
        Tf15MinuteIsOn = false;
        Tf30MinuteIsOn = true;
        Tf1HourIsOn = false;
        Tf2HourIsOn = false;
        Tf4HourIsOn = false;
        TfTickIsOn = false;
        TfMarketDepthIsOn = false;
        Source = ServerType.None;
        TimeStart = DateTime.Now.AddDays(-5);
        TimeEnd = DateTime.Now.AddDays(5);
        MarketDepthDepth = 5;
    }
    public TimeframesToLoad LoadTimeFrames { get; set; }

    public void Load(string saveStr)
    {
        string[] saveArray = saveStr.Split('%');

        LoadTimeFrames = new()
        {
            Sec1 = Convert.ToBoolean(saveArray[1]),
            Sec2 = Convert.ToBoolean(saveArray[2]),
            Sec5 = Convert.ToBoolean(saveArray[3]),
            Sec10 = Convert.ToBoolean(saveArray[4]),
            Sec15 = Convert.ToBoolean(saveArray[5]),
            Sec20 = Convert.ToBoolean(saveArray[6]),
            Sec30 = Convert.ToBoolean(saveArray[7]),

            Min1 = Convert.ToBoolean(saveArray[8]),
            Min2 = Convert.ToBoolean(saveArray[9]),
            Min5 = Convert.ToBoolean(saveArray[10]),
            Min10 = Convert.ToBoolean(saveArray[11]),
            Min15 = Convert.ToBoolean(saveArray[12]),
            Min30 = Convert.ToBoolean(saveArray[13]),
            Hour1 = Convert.ToBoolean(saveArray[14]),
            Hour2 = Convert.ToBoolean(saveArray[15]),
            Hour4 = Convert.ToBoolean(saveArray[16]),
            Tick = Convert.ToBoolean(saveArray[17]),
            MarketDepth = Convert.ToBoolean(saveArray[18]),
        };

        Tf1SecondIsOn = Convert.ToBoolean(saveArray[1]);
        Tf2SecondIsOn = Convert.ToBoolean(saveArray[2]);
        Tf5SecondIsOn = Convert.ToBoolean(saveArray[3]);
        Tf10SecondIsOn = Convert.ToBoolean(saveArray[4]);
        Tf15SecondIsOn = Convert.ToBoolean(saveArray[5]);
        Tf20SecondIsOn = Convert.ToBoolean(saveArray[6]);
        Tf30SecondIsOn = Convert.ToBoolean(saveArray[7]);

        Tf1MinuteIsOn = Convert.ToBoolean(saveArray[8]);
        Tf2MinuteIsOn = Convert.ToBoolean(saveArray[9]);
        Tf5MinuteIsOn = Convert.ToBoolean(saveArray[10]);
        Tf10MinuteIsOn = Convert.ToBoolean(saveArray[11]);
        Tf15MinuteIsOn = Convert.ToBoolean(saveArray[12]);
        Tf30MinuteIsOn = Convert.ToBoolean(saveArray[13]);
        Tf1HourIsOn = Convert.ToBoolean(saveArray[14]);
        Tf2HourIsOn = Convert.ToBoolean(saveArray[15]);
        Tf4HourIsOn = Convert.ToBoolean(saveArray[16]);
        TfTickIsOn = Convert.ToBoolean(saveArray[17]);
        TfMarketDepthIsOn = Convert.ToBoolean(saveArray[18]);

        Enum.TryParse(saveArray[19], out Source);
        try
        {
            TimeStart = Convert.ToDateTime(saveArray[20], CultureInfo.InvariantCulture);
            TimeEnd = Convert.ToDateTime(saveArray[21], CultureInfo.InvariantCulture);
        }
        catch
        {
            TimeStart = Convert.ToDateTime(saveArray[20]);
            TimeEnd = Convert.ToDateTime(saveArray[21]);
        }

        MarketDepthDepth = Convert.ToInt32(saveArray[22]);
        NeedToUpdate = Convert.ToBoolean(saveArray[23]);

        try
        {
            TfDayIsOn = Convert.ToBoolean(saveArray[24]);
        }
        catch
        {
            // ignore
        }
    }

    public string GetSaveStr()
    {
        string result = "";

        result += Tf1SecondIsOn + "%";
        result += Tf2SecondIsOn + "%";
        result += Tf5SecondIsOn + "%";
        result += Tf10SecondIsOn + "%";
        result += Tf15SecondIsOn + "%";
        result += Tf20SecondIsOn + "%";
        result += Tf30SecondIsOn + "%";

        result += Tf1MinuteIsOn + "%";
        result += Tf2MinuteIsOn + "%";
        result += Tf5MinuteIsOn + "%";
        result += Tf10MinuteIsOn + "%";
        result += Tf15MinuteIsOn + "%";
        result += Tf30MinuteIsOn + "%";
        result += Tf1HourIsOn + "%";
        result += Tf2HourIsOn + "%";
        result += Tf4HourIsOn + "%";
        result += TfTickIsOn + "%";
        result += TfMarketDepthIsOn + "%";

        result += Source + "%";
        result += TimeStart.ToString(CultureInfo.InvariantCulture) + "%";
        result += TimeEnd.ToString(CultureInfo.InvariantCulture) + "%";
        result += MarketDepthDepth + "%";
        result += NeedToUpdate + "%";
        result += TfDayIsOn + "%";

        return result;
    }

    public DateTime TimeStart;
    public DateTime TimeEnd;
    public ServerType Source;

    public bool Tf1SecondIsOn;
    public bool Tf2SecondIsOn;
    public bool Tf5SecondIsOn;
    public bool Tf10SecondIsOn;
    public bool Tf15SecondIsOn;
    public bool Tf20SecondIsOn;
    public bool Tf30SecondIsOn;

    public bool Tf1MinuteIsOn;
    public bool Tf2MinuteIsOn;
    public bool Tf5MinuteIsOn;
    public bool Tf10MinuteIsOn;
    public bool Tf15MinuteIsOn;
    public bool Tf30MinuteIsOn;
    public bool Tf1HourIsOn;
    public bool Tf2HourIsOn;
    public bool Tf4HourIsOn;
    public bool TfDayIsOn;
    public bool TfTickIsOn;
    public bool TfMarketDepthIsOn;
    public int MarketDepthDepth;
    public bool NeedToUpdate;
}

public class TimeframesToLoad
{
    public bool Sec1 { get; set; } = false;
    public bool Sec2 { get; set; } = false;
    public bool Sec5 { get; set; } = false;
    public bool Sec10 { get; set; } = false;
    public bool Sec15 { get; set; } = false;
    public bool Sec20 { get; set; } = false;
    public bool Sec30 { get; set; } = false;
    public bool Min1 { get; set; } = false;
    public bool Min2 { get; set; } = false;
    public bool Min5 { get; set; } = false;
    public bool Min10 { get; set; } = false;
    public bool Min15 { get; set; } = false;
    public bool Min30 { get; set; } = false;
    public bool Hour1 { get; set; } = false;
    public bool Hour2 { get; set; } = false;
    public bool Hour4 { get; set; } = false;
    public bool Day { get; set; } = false;

    public bool Tick { get; set;} = false;
    public bool MarketDepth { get; set; } = false;
}
