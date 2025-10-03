using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OsEngine.Models.Optimizer;

public partial class Parameter : ObservableObject
{
    [ObservableProperty]
    public bool isEnabled = false;
    // public bool IsEnabled {
    //     get;
    //     set
    //     {
    //         if (field == value) return;
    //         field = value;
    //         OnPropertyChanged();
    //     }
    // } = false;

    public string Name { get; set; }
    public string Type { get; set; }
    public object Default { get; set; }

    [ObservableProperty]
    public decimal? start;
    [ObservableProperty]
    public decimal? increment;
    [ObservableProperty]
    public decimal? end;

    // public decimal? Start {
    //     get;
    //     set
    //     {
    //         if (field == value) return;
    //         field = value;
    //         OnPropertyChanged();
    //     }
    // }

    // public decimal? Increment {
    //     get;
    //     set
    //     {
    //         if (field == value) return;
    //         field = value;
    //         OnPropertyChanged();
    //     }
    // }

    // public decimal? End {
    //     get;
    //     set
    //     {
    //         if (field == value) return;
    //         field = value;
    //         OnPropertyChanged();
    //     }
    // }
}
