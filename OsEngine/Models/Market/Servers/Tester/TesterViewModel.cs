// using OsEngine.Models.Market.Servers.Tester;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using OsEngine.Models.Market.Servers.Tester;

namespace OsEngine.ViewModels.Market.Servers.Tester;

public partial class TesterViewModel : BaseViewModel
{
    public static TesterDataType[] TesterDataTypes { get; } =
        Enum.GetValues<TesterDataType>()
        .Where(v => v != TesterDataType.None
                && v != TesterDataType.Tick
                && v != TesterDataType.OnlyReadyCandles
                && v != TesterDataType.MarketDepth)
        .ToArray();

    public static OrderExecutionType[] OrderExecutionTypes { get; } =
        Enum.GetValues<OrderExecutionType>();

    public TesterDataType TesterDataType { get; set; } = TesterDataType.Candle;
    public OrderExecutionType OrderExecutionType { get; set; }

    public ObservableCollection<NonTradePeriod> NonTradePeriods { get; } = [];
    public ObservableCollection<OrderClearing> OrderClearings { get; } = [];
    public ObservableCollection<SetFolder> SetFolders { get; } = [];
    public List<string> Sets { get; private set; }

    public string FolderPath
    {
        get;
        set;
    } = null;

    public decimal StartDeposit { get; set; } = 1_000_000m;

    [ObservableProperty]
    public bool isPause = true;

    public TesterServer TesterServer { get; set; }

    // TesterServer server
    public TesterViewModel()
    {
        TesterServer = new()
        {
            ClearingTimes = OrderClearings,
            NonTradePeriods = NonTradePeriods,
        };
        CheckSet();
    }

    public void ToggleStartPause()
    {

        if (TesterServer.Regime == TesterRegime.NotActive)
        {
            TesterServer.Regime = TesterRegime.Play;
            StartTesting();
            return;
        }

        if (TesterServer.Regime == TesterRegime.Play)
        {
            TesterServer.Regime = TesterRegime.Pause;
        }
        else
        {
            TesterServer.Regime = TesterRegime.Play;
        }
    }

    public void StartTesting()
    {
        // IsPause = !IsPause;
        // Console.WriteLine(IsPause);
        // if (TesterServer.Regime == TesterRegime.NotActive)
        // {
        //     return;
        // }
        // if (TesterServer.Regime == TesterRegime.Play)
        // {
        //     TesterServer.Regime = TesterRegime.Pause;
        // }
        // else
        // {
        //     TesterServer.Regime = TesterRegime.Play;
        // }
    }

    public void PauseTesting()
    {

    }

    public void StopTesting()
    {
        TesterServer.Regime = TesterRegime.NotActive;
    }

    public void FastForwardBy()
    {

    }

    public void FastForwardToEnd()
    {

    }

    public void CreateNewClearing()
    {
        OrderClearing newClearing = new()
        {
            Time = new TimeSpan(19, 0, 0)
        };
        TesterServer.ClearingTimes.Add(newClearing);
        TesterServer.SaveClearingInfo();
    }

    public void RemoveClearing(OrderClearing clearingTime)
    {

        TesterServer.ClearingTimes.Remove(clearingTime);
        TesterServer.SaveClearingInfo();
    }

    public void CreateNewNonTradePeriod()
    {
        NonTradePeriod newClearing = new();

        TesterServer.NonTradePeriods.Add(newClearing);
        TesterServer.SaveNonTradePeriods();
    }

    public void RemoveNonTradePeriod(NonTradePeriod nonTradePeriod)
    {
        TesterServer.NonTradePeriods.Remove(nonTradePeriod);
        TesterServer.SaveNonTradePeriods();
    }



    private void CheckSet()
    {
        string[] folders = Directory.GetDirectories($"Data{Path.DirectorySeparatorChar}");

        // FIX:
        // if (folders.Length == 0)
        // {
        //     SendLogMessage(OsLocalization.Market.Message25, LogMessageType.System);
        // }

        List<string> sets = [];

        for (int i = 0; i < folders.Length; i++)
        {
            var parts = folders[i].Split('_');
            if (parts.Length == 2)
            {
                sets.Add(parts[1]);
                // SendLogMessage("Найден сет: " + folders[i].Split('_')[1], LogMessageType.System);
            }
        }

        if (sets.Count == 0)
        {
            // SendLogMessage(OsLocalization.Market.Message25, LogMessageType.System);
        }
        Sets = sets;
    }

    public async void SetFolder(Window win)
    {
        var folder = await win.StorageProvider.OpenFolderPickerAsync(
                new()
                {
                    AllowMultiple = false,
                    Title = "Select Folder with Data"
                });

        if (folder.Count == 1)
        {
            FolderPath = folder[0].Path.AbsolutePath;
        }
        TesterServer.ReloadSecurities();
        // Task.Run(TesterServer.ReloadSecurities);
    }

    public void OnSetSelected(string set)
    {
        TesterServer.ReloadSecurities();
    }
}

public class SetFolder
{
    public string FilePath;
    public string Name;
    public DataSourceType FolderType;
    public bool IsSet => FolderType == DataSourceType.Set;

    public enum DataSourceType
    {
        Set,
        Folder,
    }
}
