using CommunityToolkit.Mvvm.ComponentModel;
using OsEngine.ViewModels.OsConverter;
using OsEngine.ViewModels.Data;
using OsEngine.Models.Entity;
using Avalonia.Controls;
using System;
using OsEngine.Views.Terminal;

namespace OsEngine.ViewModels;

public partial class MainWindowViewModel : BaseViewModel
{
    private Window window;
    public OsConverterViewModel CandleConverter { get; set; }
    public OsDataViewModel DataLoader { get; set; }

    // public OptimizerViewModel Optimizer { get; set; }
    // public TraderViewModel Trader { get; set; }
    // public TesterViewModel Tester { get; set; }

    [ObservableProperty]
    public object currentViewModel;

    private void ChangeViewToDataLoader()
    {
        // DataLoader ??= new();
        // CurrentViewModel = DataLoader;
    }

    private void ChangeViewToCandleConverter()
    {
        CandleConverter ??= new();
        CurrentViewModel = CandleConverter;
    }

    private void ChangeViewToOptimizer()
    {
        // Optimizer ??= new();
        // CurrentViewModel = Optimizer;
    }

    private void ChangeViewToTrader()
    {
        // Trader ??= new();
        // CurrentViewModel = Trader;
    }

    private void ChangeViewToTester()
    {
        // Tester ??= new();
        // CurrentViewModel = Tester;
    }

    public void OpenDataLoader()
    {

    }

    public void OpenTester()
    {
        try
        {
            // _startProgram = StartProgram.IsTester;
            window.Hide();
            // TesterUiLight candleOneUi = new();
            TerminalLight terminalLight = new();
            terminalLight.Closed += delegate
            {
                window.Show();
            };
            terminalLight.Show();

            // candleOneUi.ShowDialog(this);
            // Close();
            // ProccesIsWorked = false;
            // Thread.Sleep(5000);
        }
        catch (Exception error)
        {
            MessageBox.Show(error.ToString());
        }
    }

    public void OpenOptimizer()
    {

    }

    public void OpenDataConverter()
    {

    }
}
