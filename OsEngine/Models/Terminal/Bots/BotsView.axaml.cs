using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace OsEngine.Views.Terminal;

public partial class BotView : Window
{
    public BotView()
    {
        InitializeComponent();
    }

    public void TogglePane_Click(object sender, RoutedEventArgs e)
    {
        Pane.IsPaneOpen = !Pane.IsPaneOpen;
    }

    private ToggleButton checkedButton;

    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        var currentButton = sender as ToggleButton;

        if (currentButton == checkedButton)
        {
            Pane.IsPaneOpen = !Pane.IsPaneOpen;
            return;
        }

        if (checkedButton != null)
            checkedButton.IsChecked = false;

        checkedButton = currentButton;
        Pane.IsPaneOpen = true;

        // Uncheck all other buttons
        // foreach (var child in ((StackPanel)currentButton.Parent).Children)
        // {
        //     if (child is ToggleButton button && button != currentButton)
        //     {
        //         button.IsChecked = false;
        //     }
        // }
    }
}
