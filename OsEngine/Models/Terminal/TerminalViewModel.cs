using System.Collections.ObjectModel;
using System.Reflection;
using OsEngine.Models.Terminal;
using OsEngine.Views.Market.Servers.Tester;
using OsEngine.Views.Terminal;

namespace OsEngine.ViewModels.Terminal;

public class TerminalViewModel : BaseViewModel
{
    private TesterWindow _testerWindow;
    public ObservableCollection<BaseStrategy> Strategies { get; } = [];
    // public ObservableCollection<Bot> Bots { get; } = [];
    // public ObservableCollection<Bot> ActivePositions { get; } = [];
    // public ObservableCollection<Bot> HistoricalPositions { get; } = [];
    // public ObservableCollection<Bot> ActiveOrders { get; } = [];
    // public ObservableCollection<Bot> HistoricalOrders { get; } = [];

    public void AddBotCommand()
    {
        new AddStrategyWindow().Show();
        var v = new BotView();
        v.Show();
    }

    public void ShowTesterSettings()
    {
        if (_testerWindow != null)
        {
            _testerWindow.Activate();
            return;
        }

        var win = new TesterWindow();
        _testerWindow = win;
        win.Closed += (s, e) => _testerWindow = null;
        win.Show();
    }

    public void DeleteStrategy(BaseStrategy strategy)
    {
        
    }
}
