using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using OsEngine.Models.Entity;
using OsEngine.Models.Optimizer;
using Parameter = OsEngine.Models.Optimizer.Parameter;

namespace OsEngine.ViewModels.Optimizer;

public partial class OptimizerViewModel : BaseViewModel
{
    [ObservableProperty]
    public int botsNumber = 0;
    // public int BotsNumber { get; protected set; } = 0;

    public ObservableCollection<Parameter> Parameters { get; } = [];

    public List<IStrategyParameter> GetParameters()
    {
        return [];
    }

    public void SaveParameters()
    {

    }

    public void LoadParameters()
    {

    }

    public int GetMaxBotsCount()
    {
        if (Parameters.Count == 0) { return 0; }

        int iterations = 2 * IterationCount;
        if (LastInSample) { iterations--; }

        return BotCountOneFaze(Parameters) * iterations;
    }

    public int BotCountOneFaze(ObservableCollection<Parameter> parameters)
    {
        int combinations = 1;
        foreach (Parameter p in parameters)
        {
            // NOTE: Maybe excessive
            if (!p.IsEnabled || p.Start == null || p.End == null || p.Increment == null || p.Increment == 0)
                continue;
            combinations *= (int)((p.End - p.Start) / p.Increment) + 1;

        }
        return combinations;
        // for (int i = 0; i < allParam.Count; i++)
        // {
        //     if (allParam[i].Type == StrategyParameterType.Int)
        //     {
        //         ((StrategyParameterInt)allParam[i]).ValueInt = ((StrategyParameterInt)allParam[i]).ValueIntStart;
        //     }
        //     if (allParam[i].Type == StrategyParameterType.Decimal)
        //     {
        //         ((StrategyParameterDecimal)allParam[i]).ValueDecimal = ((StrategyParameterDecimal)allParam[i]).ValueDecimalStart;
        //     }
        //     if (allParam[i].Type == StrategyParameterType.DecimalCheckBox)
        //     {
        //         ((StrategyParameterDecimalCheckBox)allParam[i]).ValueDecimal = ((StrategyParameterDecimalCheckBox)allParam[i]).ValueDecimalStart;
        //     }
        // }
        //
        // // 1 consider how many passes we need to do in the first phase/
        // // 1 считаем сколько проходов нам нужно сделать в первой фазе
        //
        // List<IStrategyParameter> optimizedParamToCheckCount = [];
        //
        // for (int i = 0; i < allParam.Count; i++)
        // {
        //     if (allOptimezedParam[i])
        //     {
        //         optimizedParamToCheckCount.Add(allParam[i]);
        //         ReloadParam(allParam[i]);
        //     }
        // }
        //
        // optimizedParamToCheckCount = CopyParameters(optimizedParamToCheckCount);
        //
        // int countBots = 0;
        //
        // bool isStart = true;
        //
        // while (true)
        // {
        //     if (countBots > 5000000)
        //     {
        //         SendLogMessage("Iteration count > 5000000. Warning!!!", LogMessageType.Error);
        //         return countBots;
        //     }
        //
        //     bool isAndOfFaze = false; // all parameters passed/все параметры пройдены
        //
        //     for (int i2 = 0; i2 < optimizedParamToCheckCount.Count + 1; i2++)
        //     {
        //         if (i2 == optimizedParamToCheckCount.Count)
        //         {
        //             isAndOfFaze = true;
        //             break;
        //         }
        //
        //         if (isStart)
        //         {
        //             countBots++;
        //             isStart = false;
        //             break;
        //         }
        //
        //         if (optimizedParamToCheckCount[i2].Type == StrategyParameterType.Int)
        //         {
        //             StrategyParameterInt parameter = (StrategyParameterInt)optimizedParamToCheckCount[i2];
        //
        //             if (parameter.ValueInt < parameter.ValueIntStop)
        //             {
        //                 // the current index can increment the value
        //                 // по текущему индексу можно приращивать значение
        //                 parameter.ValueInt = parameter.ValueInt + parameter.ValueIntStep;
        //                 if (i2 > 0)
        //                 {
        //                     for (int i3 = 0; i3 < i2; i3++)
        //                     {
        //                         // reset all previous parameters to zero
        //                         // сбрасываем все предыдущие параметры в ноль
        //                         ReloadParam(optimizedParamToCheckCount[i3]);
        //                     }
        //                 }
        //                 countBots++;
        //                 break;
        //             }
        //         }
        //         else if (optimizedParamToCheckCount[i2].Type == StrategyParameterType.Decimal
        //                 )
        //         {
        //             StrategyParameterDecimal parameter = (StrategyParameterDecimal)optimizedParamToCheckCount[i2];
        //
        //             if (parameter.ValueDecimal < parameter.ValueDecimalStop)
        //             {
        //                 // at the current index you can increment the value
        //                 // по текущему индексу можно приращивать значение
        //                 parameter.ValueDecimal = parameter.ValueDecimal + parameter.ValueDecimalStep;
        //                 if (i2 > 0)
        //                 {
        //                     for (int i3 = 0; i3 < i2; i3++)
        //                     {
        //                         // reset all previous parameters to zero
        //                         // сбрасываем все предыдущие параметры в ноль
        //                         ReloadParam(optimizedParamToCheckCount[i3]);
        //                     }
        //                 }
        //                 countBots++;
        //                 break;
        //             }
        //         }
        //         else if (optimizedParamToCheckCount[i2].Type == StrategyParameterType.DecimalCheckBox
        //                 )
        //         {
        //             StrategyParameterDecimalCheckBox parameter = (StrategyParameterDecimalCheckBox)optimizedParamToCheckCount[i2];
        //
        //             if (parameter.ValueDecimal < parameter.ValueDecimalStop)
        //             {
        //                 // at the current index you can increment the value
        //                 // по текущему индексу можно приращивать значение
        //                 parameter.ValueDecimal = parameter.ValueDecimal + parameter.ValueDecimalStep;
        //                 if (i2 > 0)
        //                 {
        //                     for (int i3 = 0; i3 < i2; i3++)
        //                     {
        //                         // reset all previous parameters to zero
        //                         // сбрасываем все предыдущие параметры в ноль
        //                         ReloadParam(optimizedParamToCheckCount[i3]);
        //                     }
        //                 }
        //                 countBots++;
        //                 break;
        //             }
        //         }
        //     }
        //
        //     if (isAndOfFaze)
        //     {
        //         break;
        //     }
        // }
        //
        // return countBots;
    }

    public void UpdateBotsNumberCommand()
    {
        if (Parameters.Count == 0) {
            BotsNumber = 0;
            return;
        }

        int iterations = 2 * IterationCount;
        if (LastInSample) { iterations--; }

        BotsNumber = BotCountOneFaze(Parameters) * iterations;
    }

    // private Parameter_Changed()
    // {
    //
    // }
}
