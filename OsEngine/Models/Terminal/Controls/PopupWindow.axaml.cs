using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace OsEngine.Views.UserControls;

public partial class PopupWindow : UserControl
{
    public static readonly StyledProperty<double> HorizontalOffsetProperty =
        AvaloniaProperty.Register<PopupWindow, double>(nameof(HorizontalOffset), defaultValue: 0);

    public double HorizontalOffset
    {
        get => GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public static readonly StyledProperty<double> VerticalOffsetProperty =
        AvaloniaProperty.Register<PopupWindow, double>(nameof(VerticalOffset), defaultValue: 0);

    public double VerticalOffset
    {
        get => GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public PopupWindow()
    {
        InitializeComponent();
    }

    public void Close()
    {
        // Background= new BackGround;
        // Border
        Popup.IsOpen = false;
    }
    public void Toggle()
    {
        Popup.IsOpen = !Popup.IsOpen;
    }
}
