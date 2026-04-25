using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvaloniaNovel.ViewModels;

namespace AvaloniaNovel.ViewModels;

public class IndexToBoolConverter : IValueConverter
{
    public static readonly IndexToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index && parameter is string paramStr && int.TryParse(paramStr, out int targetIndex))
        {
            return index == targetIndex;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected && parameter is string paramStr && int.TryParse(paramStr, out int targetIndex))
        {
            return targetIndex;
        }
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
