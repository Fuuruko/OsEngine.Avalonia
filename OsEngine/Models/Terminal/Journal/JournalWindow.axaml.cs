using Avalonia.Controls;
using Avalonia.Interactivity;

namespace OsEngine.Views.Terminal;

public partial class JournalWindow : Window
{
    public JournalWindow()
    {
        InitializeComponent();
    }

    public void SplitPane_Toggle(object sender, RoutedEventArgs e)
    {
        JournalSplit.IsPaneOpen = !JournalSplit.IsPaneOpen;
    }
}
