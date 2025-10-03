using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;
using OsEngine.Models.Entity;

namespace OsEngine.Converters;

public class NameAttributeToStringConverter : IValueConverter
{
    private static readonly Dictionary<object, string> _names = [];
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (_names.TryGetValue(value, out string name))
        {
            return name;
        }
        else
        {
            _names[value] = ((Type)value).GetCustomAttribute<NameAttribute>()?.Name ?? ((Type)value).Name;
            return _names[value];
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public static class Converters
{
    public static FuncValueConverter<object, string> NameAttributeToString { get; } =
        new(ConvertNameAttributeToString);

    private static readonly Dictionary<object, string> _names = [];
    
    private static string ConvertNameAttributeToString(object value)
    {
        if (_names.TryGetValue(value, out string name))
        {
            return name;
        }
        else
        {
            if (value is Enum enumValue)
            {
                var enumName = enumValue.ToString();
                var field = enumValue.GetType().GetField(enumName);
                _names[value] = field.GetCustomAttribute<NameAttribute>()?.Name ?? enumName;
                return _names[value];
            }
            Console.WriteLine(value);
            Type type = value is Type type_ ? type_ : value.GetType();
            _names[value] = type.GetCustomAttribute<NameAttribute>()?.Name ?? type.Name;
            Console.WriteLine(_names[value]);
            return _names[value];
        }
    }
}

