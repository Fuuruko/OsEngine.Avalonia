using CommunityToolkit.Mvvm.ComponentModel;
using CandleConverter = OsEngine.Models.OsConverter.OsCandleConverter;
using TimeFrame = OsEngine.Models.Entity.TimeFrame;

namespace OsEngine.ViewModels.OsConverter;

public partial class OsCandleConverterViewModel : BaseViewModel
{
    public TimeFrame[] ConvertFromTimeFrame { get; } = [
        TimeFrame.Tick,
        TimeFrame.Min1,
        TimeFrame.Min5,
    ];

    public TimeFrame[] ConvertToTimeFrame { get; } = [
        TimeFrame.Min5,
        TimeFrame.Min10,
        TimeFrame.Min15,
        TimeFrame.Min30,
    ];

    // private static TimeFrame[] TickOutputTimeFrames = [
    //     TimeFrame.Sec1,
    //     TimeFrame.Sec2,
    //     TimeFrame.Sec5,
    //     TimeFrame.Sec10,
    //     TimeFrame.Sec15,
    //     TimeFrame.Sec20,
    //     TimeFrame.Sec30,
    //     TimeFrame.Min1,
    //     TimeFrame.Min2,
    //     TimeFrame.Min3,
    //     TimeFrame.Min5,
    //     TimeFrame.Min10,
    //     TimeFrame.Min15,
    //     TimeFrame.Min20,
    //     TimeFrame.Min30,
    // ];

    // private static TimeFrame[] CandleOutputTimeFrames = [
    //     TimeFrame.Min5,
    //     TimeFrame.Min10,
    //     TimeFrame.Min15,
    //     TimeFrame.Min30,
    // ];

    // public void OsConverterViewModel()
    // {
    //     // ConvertToTimeFrame = TickOutputTimeFrames;
    // }


    public void StartConvert(TimeFrame tf, string sourceFilePath, string outputFilePath)
        => CandleConverter.ConvertFile(tf, sourceFilePath, outputFilePath);

    // public void ChangeSourceTimeFrame(TimeFrame tf)
    // {
    //     if (tf == TimeFrame.Tick)
    //         ConvertToTimeFrame = TickOutputTimeFrames;
    //     else
    //         ConvertToTimeFrame = CandleOutputTimeFrames;
    // }
}
