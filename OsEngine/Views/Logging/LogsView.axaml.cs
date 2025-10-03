using Avalonia.Controls;

namespace OsEngine.Views.Logging;

public partial class LogsView : UserControl
{
    // public static readonly StyledProperty<IEnumerable> ItemsSourceProperty =
    //     AvaloniaProperty.Register<LogDataGrid, IEnumerable>(nameof(ItemsSource));
    //
    // public IEnumerable ItemsSource
    // {
    //     get => GetValue(ItemsSourceProperty);
    //     set => SetValue(ItemsSourceProperty, value);
    // }

    public LogsView()
    {
        InitializeComponent();
    }

    // private void InitializeComponent()
    // {
    //     AvaloniaXamlLoader.Load(this);
    // }

}
