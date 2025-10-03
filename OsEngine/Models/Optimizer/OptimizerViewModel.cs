using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;
using OsEngine.Models.Optimizer;
using Parameter = OsEngine.Models.Optimizer.Parameter;

namespace OsEngine.ViewModels.Optimizer;

// NOTE: Add Timer for properties that update view
public partial class OptimizerViewModel : BaseViewModel
{
    public static int[] Threads => [.. Enumerable.Range(1, 50)];

    private int threadsNum = 1;

    public int ThreadsNum
    {
        get => threadsNum;
        set => SetProperty(ref threadsNum, value);
    }

    public OptimizerViewModel()
    {
        Load();

        Parameters.CollectionChanged += (s, e) =>
        {
            ((Parameter)e.NewItems[0]).PropertyChanged += (s, e) =>
            {
                UpdateBotsNumberCommand();
            };

        };

        Parameters.Add(
                new() { Start = 1, End = 10, Increment = 2 });
        Parameters.Add(
                new() { Start = 3, End = 34, Increment = 3 });
        Parameters.Add(
                new() { Start = 1, End = 1, Increment = 1 });

        CreatePhazesCommand();
    }

    private void SetProperty<T>(ref T field, T value)
    {
        field = value;
        Save();
    }

    public void StartOptimizerCommand()
    {

    }

    public void ShowResultsCommand()
    {

    }

    public void SendLogMessage(string message, LogMessageType type)
    {
        LogMessageEvent?.Invoke(message, type);
    }

    public event Action<string, LogMessageType> LogMessageEvent;
}
