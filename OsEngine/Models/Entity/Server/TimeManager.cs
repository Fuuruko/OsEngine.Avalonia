using System;

namespace OsEngine.Models.Entity.Server;

public static class TimeManager
{
    private static readonly DateTime _epoch = new(1970, 1, 1);

    // TODO: Turn to DateTime extention
    public static DateTime GetExchangeTime(string needTimeZone)
    {
        TimeZoneInfo neededTimeZone = TimeZoneInfo.FindSystemTimeZoneById(needTimeZone);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, neededTimeZone);
    }

    public static DateTime GetDateTimeFromTimeStamp(long timeStamp)
    {
        return _epoch.AddMilliseconds(timeStamp);
    }

    public static DateTime GetDateTimeFromTimeStampSeconds(long timeStamp)
    {
        return _epoch.AddSeconds(timeStamp);
    }

    public static long GetUnixTimeStampSeconds()
    {
        return Convert.ToInt64(GetUnixTimeStamp().TotalSeconds);
    }

    public static long GetUnixTimeStampMilliseconds()
    {
        return Convert.ToInt64(GetUnixTimeStamp().TotalMilliseconds);
    }

    public static int GetTimeStampSecondsToDateTime(DateTime time)
    {
        return (int)(time - _epoch).TotalSeconds;
    }

    public static long GetTimeStampMilliSecondsToDateTime(DateTime time)
    {
        return (long)(time - _epoch).TotalMilliseconds;
    }

    public static TimeSpan GetTimeStampFromDateTime(DateTime time)
    {
        return time - _epoch;
    }

    private static TimeSpan GetUnixTimeStamp()
    {
        return DateTime.UtcNow - _epoch;
    }
}
