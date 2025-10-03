using System;
using System.Collections.Generic;
using System.Linq;

namespace OsEngine.Models.Entity;

public enum TimeFrame
{
    // NOTE: MarketDepth Used only in Data loading
    MarketDepth = -1,
    Tick = 0,
    Sec1 = 1,
    Sec2 = 2,
    Sec5 = 5,
    Sec10 = 10,
    Sec15 = 15,
    Sec20 = 20,
    Sec30 = 30,
    Min1 = 60,
    Min2 = 2 * Min1,
    Min3 = 3 * Min1,
    Min5 = 5 * Min1,
    Min10 = 10 * Min1,
    Min15 = 15 * Min1,
    Min20 = 20 * Min1,
    Min30 = 30 * Min1,
    Min45 = 45 * Min1,
    Hour1 = 60 * Min1,
    Hour2 = 2 * Hour1,
    Hour4 = 4 * Hour1,
    Day = 24 * Hour1,
}

static class TimeFrameMethods
{
    // TODO: Can be simplified
    // private static readonly Dictionary<TimeFrame, TimeSpan> _timeFrameToTimeSpan = new()
    // {
    //     { TimeFrame.Sec1,  new (0, 0, 0, 1) },
    //     { TimeFrame.Sec2,  new (0, 0, 0, 2) },
    //     { TimeFrame.Sec5,  new (0, 0, 0, 5) },
    //     { TimeFrame.Sec10, new (0, 0, 0, 10) },
    //     { TimeFrame.Sec15, new (0, 0, 0, 15) },
    //     { TimeFrame.Sec20, new (0, 0, 0, 20) },
    //     { TimeFrame.Sec30, new (0, 0, 0, 30) },
    //     { TimeFrame.Min1,  new (0, 0, 1, 0) },
    //     { TimeFrame.Min2,  new (0, 0, 2, 0) },
    //     { TimeFrame.Min3,  new (0, 0, 3, 0) },
    //     { TimeFrame.Min5,  new (0, 0, 5, 0) },
    //     { TimeFrame.Min10, new (0, 0, 10, 0) },
    //     { TimeFrame.Min15, new (0, 0, 15, 0) },
    //     { TimeFrame.Min20, new (0, 0, 20, 0) },
    //     { TimeFrame.Min30, new (0, 0, 30, 0) },
    //     { TimeFrame.Min45, new (0, 0, 45, 0) },
    //     { TimeFrame.Hour1, new (0, 1, 0, 0) },
    //     { TimeFrame.Hour2, new (0, 2, 0, 0) },
    //     { TimeFrame.Hour4, new (0, 4, 0, 0) },
    //     { TimeFrame.Day,   new (1, 0, 0, 0) },
    // };

    private static readonly Dictionary<TimeFrame, TimeSpan> _timeFrameToTimeSpan =
        Enum.GetValues<TimeFrame>()
        .Where(tf => (int)tf > 0)
        .ToDictionary(tf => tf, tf => TimeSpan.FromSeconds((int)tf));

    // public static double GetTotalMinutes(this TimeFrame tf) =>
    //     _timeFrameToTimeSpan.TryGetValue(tf, out TimeSpan value) ? value.TotalMinutes : 0;
    public static double GetTotalMinutes(this TimeFrame tf) => (int)tf / 60;

    public static double GetTotalSeconds(this TimeFrame tf) => tf != TimeFrame.MarketDepth ? (double)tf : 0;

    public static TimeSpan? GetTimeSpan(this TimeFrame tf) =>
        _timeFrameToTimeSpan.TryGetValue(tf, out TimeSpan value) ? value : null;
    // extension(TimeFrame tf)
    // {
    //     public double GetTotalMinutes => _timeFrameToTimeSpan.TryGetValue(tf, out TimeSpan value) ? value.TotalMinutes : 0
    //     public TimeSpan? GetTimeSpan =>
    //         _timeFrameToTimeSpan.TryGetValue(tf, out TimeSpan value) ? value : null
    // };

    // public static double GetTotalMinutes(this TimeFrame tf) =>
    //     _timeFrameToTimeSpan.TryGetValue(tf, out TimeSpan value) ? value.TotalMinutes : 1.0;
}
