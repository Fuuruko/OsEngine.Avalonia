using System;
using System.IO;
using Newtonsoft.Json;
using OsEngine.Models.Logging;
using OsEngine.Models.Optimizer;

namespace OsEngine.ViewModels.Optimizer;

public partial class OptimizerViewModel : BaseViewModel
{
    public void Save()
    {
        try
        {
            // TODO: Save every n seconds?
            //  Can i use OptimizerViewModel instead of settings?
            OptimizerSettings settings = new()
            {
                // StrategyName = StrategyName,
                // IsScript = IsScript,
                ThreadsNum = ThreadsNum,

                StartDeposit = StartDeposit,
                CommissionType = CommissionType,
                CommissionValue = CommissionValue,

                StartTime = StartTime,
                EndTime = EndTime,
                LastInSample = LastInSample,
                IterationCount = IterationCount,
                PercentOnFiltration = PercentOnFiltration,

                IsMinimalProfitEnabled = IsMinimalProfitEnabled,
                IsMaxDrowDownEnabled = IsMaxDrowDownEnabled,
                IsMinimalAverageProfitEnabled = IsMinimalAverageProfitEnabled,
                IsMinimalProfitFactorEnabled = IsMinimalProfitFactorEnabled,
                IsMinimalDealsEnabled = IsMinimalDealsEnabled,
                MinimalProfit = MinimalProfit,
                MaxDrowDown = MaxDrowDown,
                MinimalAverageProfit = MinimalAverageProfit,
                MinimalProfitFactor = MinimalProfitFactor,
                MinimalDeals = MinimalDeals,

                // OrderExecutionType = OrderExecutionType,
                // StopSlippage = StopSlippage,
                // LimitSlippage = LimitSlippage,
            };
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            string path = $"Engine{Path.DirectorySeparatorChar}OptimizerSettings.txt";
            File.WriteAllText(path, json);
            Console.WriteLine(DateTime.Now);
        }
        catch (Exception error)
        {
            SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void Load()
    {
        string path = $"Engine{Path.DirectorySeparatorChar}OptimizerSettings.txt";
        if (!File.Exists(path)) { return; }
        try
        {
            // TODO: There is a lot savings
            //  because of SetProperty
            string jsonFromFile = File.ReadAllText(path);

            // Deserialize the JSON string back to an object
            OptimizerSettings settings = JsonConvert.DeserializeObject<OptimizerSettings>(jsonFromFile);

            threadsNum = settings.ThreadsNum;

            startDeposit = settings.StartDeposit;
            commissionType = settings.CommissionType;
            commissionValue = settings.CommissionValue;

            startTime = settings.StartTime;
            endTime = settings.EndTime;
            lastInSample = settings.LastInSample;
            iterationCount = settings.IterationCount;
            percentOnFiltration = settings.PercentOnFiltration;

            isMinimalProfitEnabled = settings.IsMinimalProfitEnabled;
            isMaxDrowDownEnabled = settings.IsMaxDrowDownEnabled;
            isMinimalAverageProfitEnabled = settings.IsMinimalAverageProfitEnabled;
            isMinimalProfitFactorEnabled = settings.IsMinimalProfitFactorEnabled;
            isMinimalDealsEnabled = settings.IsMinimalDealsEnabled;
            minimalProfit = settings.MinimalProfit;
            maxDrowDown = settings.MaxDrowDown;
            minimalAverageProfit = settings.MinimalAverageProfit;
            minimalProfitFactor = settings.MinimalProfitFactor;
            minimalDeals = settings.MinimalDeals;
        }
        catch (Exception error)
        {
            SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }
}
