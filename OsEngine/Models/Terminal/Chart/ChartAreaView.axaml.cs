using Avalonia.Controls;
using Avalonia.Media;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace OsEngine.Views.Terminal.Chart;

public partial class ChartAreaView : UserControl
{
    public ChartAreaView()
    {
        InitializeComponent();
    }

    public void AddArea()
    {
        var ChartRow = new RowDefinition(1, GridUnitType.Star);
        var SplitterRow = new RowDefinition(new(5));

        // ChartGrid.SetRow()
        var i = ChartGrid.RowDefinitions.Count;
        ChartGrid.RowDefinitions.Add(ChartRow);

        var Chart = new CartesianChart()
        {
            FindingStrategy = LiveChartsCore.Measure.FindingStrategy.ExactMatch,

        };

        Grid.SetRow(Chart, i + 1);

        ChartGrid.RowDefinitions.Add(SplitterRow);

        var splitter = new GridSplitter()
        {
            // ResizeBehavior = ,
            ResizeDirection = GridResizeDirection.Columns,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Height = 5,
            // Foreground = Brush,
        };

        Grid.SetRow(splitter, i + 2);

        ChartGrid.Children.Add(Chart);
        ChartGrid.Children.Add(splitter);
    }
}
