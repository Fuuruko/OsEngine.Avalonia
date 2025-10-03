using Avalonia.Media;
using OsEngine.Models.Indicators;

namespace OsEngine.Models.Entity;

public interface IDrawable
{
    string Name { get; }

    bool IsVisible { get; set; }

    Color Color { get; set; }

    ChartSeriesType ChartType { get; set; }
}
