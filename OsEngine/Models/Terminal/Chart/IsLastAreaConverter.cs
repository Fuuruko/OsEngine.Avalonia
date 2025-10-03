using Avalonia.Data.Converters;
using OsEngine.ViewModels.Terminal;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OsEngine.Models.Terminal.Chart;

public class IsLastAreaConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not IList<ChartArea> chartAreas || value == null) return false;

        return chartAreas.IndexOf((ChartArea)value) == chartAreas.Count - 1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
