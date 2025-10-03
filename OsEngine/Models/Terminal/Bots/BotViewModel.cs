using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OsEngine.Models.Candles;
using OsEngine.Models.Candles.Series;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;
using OsEngine.Models.Terminal;
using OsEngine.Views.Market.Connectors;
using OsEngine.Views.Terminal;

namespace OsEngine.ViewModels.Terminal;

public partial class BotViewModel : BaseViewModel
{
    public static CommissionType[] CommissionTypes { get; } = Enum.GetValues<CommissionType>();
    public static CandleMarketDataType[] CandleMarketDataTypes { get; } = Enum.GetValues<CandleMarketDataType>();

    public static List<Type> CandleTypes { get; } = ACandlesSeriesRealization.CandleTypes;
    public Type CandleType { get; set; } = typeof(Simple);

    // public string 
    // public ObservableCollection<ViewModelBase>
    public ObservableCollection<Position> OpenPositions = [];
    public ObservableCollection<Position> ClosedPositions = [];

    public ObservableCollection<IBot> Bots = [];
    public IBot CurrentBot { get; set; }

    public ObservableCollection<Portfolio> Portfolios { get; set; } = [];
    public Portfolio Portfolio { get; set; }

    public bool IsTester { get; init; } = false;

    [ObservableProperty]
    private object selectedViewModel;

    private object MarketOfDepthView =  new DepthOfMarketView();
    private object GridView = new GridView();
    private object AlertsView = new AlertsView();
    private object ControlsView = new ControlsView();

    public BotViewModel()
    {
        Console.WriteLine(selectedViewModel);
    }


    public void MarketOfDepthSelectCommand() => SelectedViewModel = MarketOfDepthView;
    public void GridSelectCommand() => SelectedViewModel = GridView;
    public void AlertsSelectCommand() => SelectedViewModel = AlertsView;
    public void ControlsSelectCommand() => SelectedViewModel = ControlsView;

    public void PositionSupportCommand()
    {
        var view = new PositionSupportView();
        view.Show();
    }

    public void JournalShowCommand()
    {
        new JournalWindow().Show();
    }

    public void ShowDataSettings()
    {
        new ConnectorCandlesWindow(this).Show();
    }
}
