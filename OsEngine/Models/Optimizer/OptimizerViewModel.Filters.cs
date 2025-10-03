namespace OsEngine.ViewModels.Optimizer;

public partial class OptimizerViewModel : BaseViewModel
{
    private bool isMinimalProfitEnabled = false;
    private decimal minimalProfit = 10;
    private bool isMaxDrowDownEnabled = false;
    private decimal maxDrowDown = -10;
    private bool isMinimalAverageProfitEnabled = false;
    private decimal minimalAverageProfit = 0.001m;
    private bool isMinimalProfitFactorEnabled = false;
    private decimal minimalProfitFactor = 1;
    private bool isMinimalDealsEnabled = false;
    private decimal minimalDeals;

    public bool IsMinimalProfitEnabled
    {
        get => isMinimalProfitEnabled;
        set => SetProperty(ref isMinimalProfitEnabled, value);
    }

    public decimal MinimalProfit
    {
        get => minimalProfit;
        set => SetProperty(ref minimalProfit, value);
    }

    public bool IsMaxDrowDownEnabled
    {
        get => isMaxDrowDownEnabled;
        set => SetProperty(ref isMaxDrowDownEnabled, value);
    }

    public decimal MaxDrowDown
    {
        get => maxDrowDown;
        set => SetProperty(ref maxDrowDown, value);
    }

    public bool IsMinimalAverageProfitEnabled
    {
        get => isMinimalAverageProfitEnabled;
        set => SetProperty(ref isMinimalAverageProfitEnabled, value);
    }

    public decimal MinimalAverageProfit
    {
        get => minimalAverageProfit;
        set => SetProperty(ref minimalAverageProfit, value);
    }

    public bool IsMinimalProfitFactorEnabled
    {
        get => isMinimalProfitFactorEnabled;
        set => SetProperty(ref isMinimalProfitFactorEnabled, value);
    }

    public decimal MinimalProfitFactor
    {
        get => minimalProfitFactor;
        set => SetProperty(ref minimalProfitFactor, value);
    }

    public bool IsMinimalDealsEnabled
    {
        get => isMinimalDealsEnabled;
        set => SetProperty(ref isMinimalDealsEnabled, value);
    }

    public decimal MinimalDeals
    {
        get => minimalDeals;
        set => SetProperty(ref minimalDeals, value);
    }

    // public bool IsAcceptedByFilter(OptimizerReport report)
    // {
    //     if (report == null)
    //     {
    //         return false;
    //     }
    //
    //     if (IsMinimalAverageProfitEnabled
    //             && report.AverageProfitPercentOneContract < MinimalAverageProfit)
    //     {
    //         return false;
    //     }
    //
    //     if (IsMinimalProfitEnabled
    //             && report.TotalProfit < MinimalProfit)
    //     {
    //         return false;
    //     }
    //
    //     if (IsMaxDrowDownEnabled
    //             && report.MaxDrawDawn < MaxDrowDown)
    //     {
    //         return false;
    //     }
    //
    //     if (IsMinimalProfitFactorEnabled
    //             && report.ProfitFactor < MinimalProfitFactor)
    //     {
    //         return false;
    //     }
    //
    //     if (IsMinimalDealsEnabled
    //             && report.PositionsCount < MinimalDeals)
    //     {
    //         return false;
    //     }
    //
    //     return true;
    // }
}
