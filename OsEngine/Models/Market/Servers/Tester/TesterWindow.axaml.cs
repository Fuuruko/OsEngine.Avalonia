using Avalonia.Controls;
using Avalonia.Interactivity;
using OsEngine.ViewModels.Market.Servers.Tester;

namespace OsEngine.Views.Market.Servers.Tester;

public partial class TesterWindow : Window
{
    private static TesterViewModel viewModel = new();
    public TesterWindow()
    {
        InitializeComponent();
        DataContext = viewModel;
        // Prevent scroll when control focused
        ComboBoxDataType.AddHandler(PointerWheelChangedEvent,
                (s, e) => e.Handled = true,
                RoutingStrategies.Tunnel);
    }

    public async void SetFolder()
    {
       var folder = await StorageProvider.OpenFolderPickerAsync(new());

       if (folder.Count == 1)
       {
            ((TesterViewModel)DataContext).FolderPath = folder[0].Path.AbsolutePath;
       }
    }
}

