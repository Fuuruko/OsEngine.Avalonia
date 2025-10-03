using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using OsEngine.ViewModels;

namespace OsEngine.ViewModels.Optimizer;

public class CrossValidationViewModel : BaseViewModel
{

    public ISeries[] Series { get; } = [
        new RowSeries<int>
        {
            // Values = 
        }
    ];
}
