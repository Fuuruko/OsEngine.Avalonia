using System;
using OsEngine.Models.Entity;
using OsEngine.Models.Market.Servers.Tester;

namespace OsEngine.Models.Optimizer;

public class OptimizerSettings
{
    public string StrategyName;
    public bool IsScript;
    public int ThreadsNum;

    public decimal StartDeposit;
    public CommissionType CommissionType;
    public decimal CommissionValue;

    public DateTime StartTime;
    public DateTime EndTime;
    public bool LastInSample;
    public int IterationCount;
    public decimal PercentOnFiltration;

    public bool IsMinimalProfitEnabled;
    public bool IsMaxDrowDownEnabled;
    public bool IsMinimalAverageProfitEnabled;
    public bool IsMinimalProfitFactorEnabled;
    public bool IsMinimalDealsEnabled;
    public decimal MinimalProfit;
    public decimal MaxDrowDown;
    public decimal MinimalAverageProfit;
    public decimal MinimalProfitFactor;
    public decimal MinimalDeals;

    public OrderExecutionType OrderExecutionType;
    public int LimitSlippage;
    public int StopSlippage;
}
