using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace OsEngine.Views.Terminal;

public partial class ControlsView : UserControl
{
    public ControlsView()
    {
        InitializeComponent();
    }

    private void ToggleRiskManagerPopup(object sender, RoutedEventArgs e)
    {
        RiskManagerPopup.IsOpen = !RiskManagerPopup.IsOpen;
    }

    private void CloseRiskManagerPopup(object sender, RoutedEventArgs e)
    {
        RiskManagerPopup.IsOpen = false;
    }

}
