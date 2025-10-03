/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using OsEngine.Models.Entity;
using OsEngine.Models.Market.Servers;

namespace OsEngine.Views.Market.Servers;

public partial class ServerParametersWindow : Window
{
    public List<IBaseInput> Inputs { get; }
    private readonly List<BindingExpressionBase> BindingExpressions = [];

    public ServerParametersWindow(List<IBaseInput> inputs = null)
    {
        Console.WriteLine(inputs.Count);
        Inputs = inputs ?? [];
        InitializeComponent();
        DataContext = this;
        // Loaded += (s, e) => AttachValueChangedEvent(this);
    }

    // private void AttachValueChangedEvent(Control parent)
    // {
    //     // Find all NumericUpDown controls in the MainStackPanel
    //     foreach (var child in parent.GetLogicalChildren())
    //     {
    //         if (child is NumericUpDown numericUpDown)
    //         {
    //             numericUpDown.ValueChanged += NumericUpDown_ValueChanged;
    //         }
    //         else if (child is Control control) // Check if the child is a Control
    //         {
    //             // Recursively call this method for nested controls
    //             AttachValueChangedEvent(control);
    //         }
    //     }
    // }
    //
    public void Accept()
    {
        foreach (var binding in BindingExpressions)
        {
            binding.UpdateSource();
        }
        Task.Run(BaseServer.SaveServers);
        Close();
    }


    public void NumericUpDown_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
    {
        if (!e.NewValue.HasValue)
        {
            (sender as NumericUpDown).Value = e.OldValue;
        }
    }
    
    public void Template_Loaded(object s, RoutedEventArgs e)
    {
        BindingExpressionBase binding = s switch
        {
            CheckBox c => BindingOperations
                .GetBindingExpressionBase(c, CheckBox.IsCheckedProperty),
            ToggleSwitch c => BindingOperations
                .GetBindingExpressionBase(c, ToggleSwitch.IsCheckedProperty),
            NumericUpDown c => BindingOperations
                .GetBindingExpressionBase(c, NumericUpDown.ValueProperty),
            TextBox c => BindingOperations
                .GetBindingExpressionBase(c, TextBox.TextProperty),
            ComboBox c => BindingOperations
                .GetBindingExpressionBase(c, ComboBox.SelectedValueProperty),
            _ => throw new Exception("There is no template data for this type"),
        };

        BindingExpressions.Add(binding);

    }

    public void Cancel(object sender, RoutedEventArgs e) => Close();
}
