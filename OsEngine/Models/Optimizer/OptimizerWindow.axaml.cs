using System;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace OsEngine.Views.Optimizer;

public partial class OptimizerWindow : Window
{
    public OptimizerWindow()
    {
        InitializeComponent();
        Loaded += (s, e) => AttachValueChangedEvent(this);
    }

    private void AttachValueChangedEvent(Control parent)
    {
        // Find all NumericUpDown controls in the MainStackPanel
        foreach (var child in parent.GetLogicalChildren())
        {
            if (child is NumericUpDown numericUpDown)
            {
                numericUpDown.ValueChanged += NumericUpDown_ValueChanged;
            }
            else if (child is Control control) // Check if the child is a Control
            {
                // Recursively call this method for nested controls
                AttachValueChangedEvent(control);
            }
        }
    }


    public void NumericUpDown_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
    {
        if (!e.NewValue.HasValue)
        {
            (sender as NumericUpDown).Value = e.OldValue;
        }
    }
}
