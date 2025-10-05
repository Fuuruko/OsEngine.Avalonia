using Avalonia.Controls;

namespace OsEngine.Views.Market.Connectors;

public partial class ConnectorCandlesWindow : Window
{
    public ConnectorCandlesWindow()
    {
        InitializeComponent();
    }

    public ConnectorCandlesWindow(object viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

