using Avalonia.Controls;

namespace OsEngine.Views.Data;

public partial class AddServersWindow : Window
{
    public AddServersWindow()
    {
        InitializeComponent();
    }

    public AddServersWindow(object dataContext)
    {
        DataContext = dataContext;
        InitializeComponent();
    }
}

