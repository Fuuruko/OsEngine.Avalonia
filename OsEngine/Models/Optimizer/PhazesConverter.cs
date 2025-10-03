using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using OsEngine.Models.Optimizer;

namespace OsEngine.Views.Optimizer;

// public class FazeValueConverter : IValueConverter
// {
//     public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//     {
//         var data = value as ObservableCollection<OptimizerFaze>;
//         return data?.Select(d => d.Value).ToArray();
//     }
//
//     public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//     {
//         throw new NotImplementedException();
//     }
// }
//
// public class FazeCategoryConverter : IValueConverter
// {
//     public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//     {
//         var data = value as ObservableCollection<OptimizerFaze>;
//         return data?.Select(d => d.Category).ToArray();
//     }
//
//     public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//     {
//         throw new NotImplementedException();
//     }
// }
