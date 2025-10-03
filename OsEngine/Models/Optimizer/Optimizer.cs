using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Optimizer;

public partial class Optimizer : ObservableObject
{

    public void Save()
    {
        try
        {
            using StreamWriter writer = new(@"Engine\OptimizerSettings.txt", false);
            // writer.WriteLine(ThreadsCount);
            // writer.WriteLine(StrategyName);
            // writer.WriteLine(_startDeposit);
            //
            // writer.WriteLine(_filterProfitValue);
            // writer.WriteLine(_filterProfitIsOn);
            // writer.WriteLine(_filterMaxDrawDownValue);
            // writer.WriteLine(_filterMaxDrawDownIsOn);
            // writer.WriteLine(_filterMiddleProfitValue);
            // writer.WriteLine(_filterMiddleProfitIsOn);
            // writer.WriteLine(_filterProfitFactorValue);
            // writer.WriteLine(_filterProfitFactorIsOn);
            //
            // writer.WriteLine(_timeStart.ToString(CultureInfo.InvariantCulture));
            // writer.WriteLine(_timeEnd.ToString(CultureInfo.InvariantCulture));
            // writer.WriteLine(_percentOnFiltration);
            //
            // writer.WriteLine(_filterDealsCountValue);
            // writer.WriteLine(_filterDealsCountIsOn);
            // writer.WriteLine(_isScript);
            // writer.WriteLine(_iterationCount);
            // writer.WriteLine(_commissionType);
            // writer.WriteLine(_commissionValue);
            // writer.WriteLine(_lastInSample);
            // writer.WriteLine(_orderExecutionType);
            // writer.WriteLine(_slippageToSimpleOrder);
            // writer.WriteLine(_slippageToStopOrder);

            writer.Close();
        }
        catch (Exception error)
        {
            // SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    private void Load()
    {
        if (!File.Exists(@"Engine\OptimizerSettings.txt")) { return; }
        try
        {
            using StreamReader reader = new(@"Engine\OptimizerSettings.txt");
            // _threadsCount = Convert.ToInt32(reader.ReadLine());
            // _strategyName = reader.ReadLine();
            // _startDeposit = reader.ReadLine().ToDecimal();
            // _filterProfitValue = reader.ReadLine().ToDecimal();
            // _filterProfitIsOn = Convert.ToBoolean(reader.ReadLine());
            // _filterMaxDrawDownValue = reader.ReadLine().ToDecimal();
            // _filterMaxDrawDownIsOn = Convert.ToBoolean(reader.ReadLine());
            // _filterMiddleProfitValue = reader.ReadLine().ToDecimal();
            // _filterMiddleProfitIsOn = Convert.ToBoolean(reader.ReadLine());
            // _filterProfitFactorValue = reader.ReadLine().ToDecimal();
            // _filterProfitFactorIsOn = Convert.ToBoolean(reader.ReadLine());
            //
            // _timeStart = Convert.ToDateTime(reader.ReadLine(), CultureInfo.InvariantCulture);
            // _timeEnd = Convert.ToDateTime(reader.ReadLine(), CultureInfo.InvariantCulture);
            // _percentOnFiltration = reader.ReadLine().ToDecimal();
            //
            // _filterDealsCountValue = Convert.ToInt32(reader.ReadLine());
            // _filterDealsCountIsOn = Convert.ToBoolean(reader.ReadLine());
            // _isScript = Convert.ToBoolean(reader.ReadLine());
            // _iterationCount = Convert.ToInt32(reader.ReadLine());
            // _commissionType = (CommissionType)Enum.Parse(typeof(CommissionType),
            //         reader.ReadLine() ?? CommissionType.None.ToString());
            // _commissionValue = reader.ReadLine().ToDecimal();
            // _lastInSample = Convert.ToBoolean(reader.ReadLine());
            //
            // Enum.TryParse(reader.ReadLine(), out _orderExecutionType);
            // _slippageToSimpleOrder = Convert.ToInt32(reader.ReadLine());
            // _slippageToStopOrder = Convert.ToInt32(reader.ReadLine());
            //
            // reader.Close();
        }
        catch (Exception error)
        {
            //SendLogMessage(error.ToString(), LogMessageType.Error);
        }
    }
}
