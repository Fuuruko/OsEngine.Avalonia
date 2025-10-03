using System;
using System.Globalization;

namespace OsEngine.Models.Optimizer;

public class OptimizerPhaze
{
    public int Num { get; set; }

    public OptimizerPhazeType Type { get; set; }

    public DateTime StartTime
    {
        get;
        set
        {
            field = value;
            Days = Convert.ToInt32((EndTime - StartTime).TotalDays);
        }
    }

    public DateTime EndTime
    {
        get;
        set
        {
            field = value;
            Days = Convert.ToInt32((EndTime - StartTime).TotalDays);
        }
    }

    public DateTime TestStart
    {
        get;
        set
        {
            field = value;
            Days = Convert.ToInt32((EndTime - StartTime).TotalDays);
        }
    }

    public DateTime TestTime
    {
        get;
        set
        {
            field = value;
            Days = Convert.ToInt32((EndTime - StartTime).TotalDays);
        }
    }

    public int Days { get; set; }

    public string GetSaveString()
    {
        string result = "";

        result += Type.ToString() + "%";

        result += StartTime.ToString(CultureInfo.InvariantCulture) + "%";

        result += EndTime.ToString(CultureInfo.InvariantCulture) + "%";

        result += Days.ToString() + "%";

        return result;
    }

    public void LoadFromString(string saveStr)
    {
        string[] str = saveStr.Split('%');

        Enum.TryParse(str[0], out OptimizerPhazeType type);
        Type = type;

        StartTime = Convert.ToDateTime(str[1], CultureInfo.InvariantCulture);

        EndTime = Convert.ToDateTime(str[2], CultureInfo.InvariantCulture);

        Days = Convert.ToInt32(str[3]);
    }

}

public enum OptimizerPhazeType
{
    InSample,
    OutOfSample
}
