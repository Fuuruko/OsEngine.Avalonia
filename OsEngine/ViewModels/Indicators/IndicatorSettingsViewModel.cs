using System.Collections.ObjectModel;
using Avalonia.Media;
using OsEngine.Models.Indicators;

namespace OsEngine.ViewModels.Indicators;

public partial class IndicatorSettingsViewModel : BaseViewModel
{
    // public delegate object IndicatorCreator(string param1, bool param2, );
    // private static Dictionary<string, IIndicator> _indicatorMap = new()
    // {
    //     { "DonchianChannel", (p1, p2, p3) => new DonchianChannel(p1, p2, p3) },
    // };
    public ObservableCollection<VisualSetting> VisualSettings =  [];
    public ObservableCollection<Setting> Settings =  [];
    public ObservableCollection<SubIndicatorSetting> SubIndicatorSettings =  [];

    public void Load()
    {

    }
}

public class VisualSetting(string name, Color color, IndicatorChartPaintType type, bool show)
{
    string Name { get; } = name;
    Color Color { get; set; } = color;
    IndicatorChartPaintType Type { get; set; } = type;
    bool Show { get; set; } = show;
}

public class Setting(string name, object value)
{
    string Name { get; } = name;
    object Value { get; set; } = value;
}

public class SubIndicatorSetting(string name, IIndicator indicator)
{
    string Name { get; } = name;
    object Indicator { get; } = indicator;
}
