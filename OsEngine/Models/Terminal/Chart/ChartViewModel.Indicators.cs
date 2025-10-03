using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using OsEngine.Models.Indicators;

namespace OsEngine.ViewModels.Terminal;

public partial class ChartViewModel : BaseViewModel
{
    public List<IIndicator> Indicators { get; } = [];

}
