using System;

namespace OsEngine.Models.Entity;

public class Funding
{
    public string SecurityNameCode
    {
        get;
        set => field = string.Intern(value);
    }

    public decimal CurrentValue;

    public DateTime NextFundingTime = new(1970, 1, 1, 0, 0, 0);

    public DateTime TimeUpdate = new(1970, 1, 1, 0, 0, 0);

    public decimal PreviousValue;

    public DateTime PreviousFundingTime = new(1970, 1, 1, 0, 0, 0);

    public int FundingIntervalHours;

    public decimal MaxFundingRate;

    public decimal MinFundingRate;
}
