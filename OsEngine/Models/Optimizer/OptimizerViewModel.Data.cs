
using System;
using OsEngine.Models.Entity;

namespace OsEngine.ViewModels.Optimizer;

public partial class OptimizerViewModel : BaseViewModel
{
    public static CommissionType[] CommissionTypes => Enum.GetValues<CommissionType>();

    private decimal commissionValue = 0;
    private CommissionType commissionType = CommissionType.None;
    private decimal startDeposit = 1000000m;

    public CommissionType CommissionType
    {
        get => commissionType;
        set => SetProperty(ref commissionType, value);
    }

    public decimal CommissionValue
    {
        get => commissionValue;
        set => SetProperty(ref commissionValue, value);
    }

    public decimal StartDeposit
    {
        get => startDeposit;
        set => SetProperty(ref startDeposit, value);
    }

}
