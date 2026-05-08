using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaNovel.Models;

namespace AvaloniaNovel.ViewModels;

public class StringNullOrEmptyConverter : IValueConverter
{
    public static readonly StringNullOrEmptyConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string str && !string.IsNullOrEmpty(str);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PromptTemplateTypeColorConverter : IValueConverter
{
    public static readonly PromptTemplateTypeColorConverter Instance = new();

    private static readonly Dictionary<PromptTemplateType, SolidColorBrush> Colors = new()
    {
        [PromptTemplateType.System] = new SolidColorBrush(Color.FromRgb(99, 102, 241)),
        [PromptTemplateType.Outline] = new SolidColorBrush(Color.FromRgb(236, 72, 153)),
        [PromptTemplateType.Chapter] = new SolidColorBrush(Color.FromRgb(16, 185, 129))
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is PromptTemplateType type && Colors.TryGetValue(type, out var brush)
            ? brush
            : new SolidColorBrush(Colors[PromptTemplateType.System].Color);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ChapterStatusColorConverter : IValueConverter
{
    public static readonly ChapterStatusColorConverter Instance = new();

    private static readonly Dictionary<ChapterStatus, SolidColorBrush> Colors = new()
    {
        [ChapterStatus.Outline] = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
        [ChapterStatus.Writing] = new SolidColorBrush(Color.FromRgb(245, 158, 11)),
        [ChapterStatus.Completed] = new SolidColorBrush(Color.FromRgb(16, 185, 129))
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ChapterStatus status && Colors.TryGetValue(status, out var brush)
            ? brush
            : new SolidColorBrush(Colors[ChapterStatus.Outline].Color);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PromptTemplateTypeConverter : IValueConverter
{
    public static readonly PromptTemplateTypeConverter Instance = new();

    private static readonly Dictionary<PromptTemplateType, string> DisplayNames = new()
    {
        [PromptTemplateType.System] = "系统人设",
        [PromptTemplateType.Outline] = "大纲生成",
        [PromptTemplateType.Chapter] = "章节写作"
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is PromptTemplateType type && DisplayNames.TryGetValue(type, out var name)
            ? name
            : value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public static readonly InverseBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }
}

public class TemplateTypeFilterBackgroundConverter : IValueConverter
{
    public static readonly TemplateTypeFilterBackgroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return new SolidColorBrush(Color.FromRgb(37, 42, 57));

        if (value is PromptTemplateType selectedType && parameter is string paramStr)
        {
            if (Enum.TryParse<PromptTemplateType>(paramStr, out var parsedType) && selectedType == parsedType)
            {
                return new SolidColorBrush(Color.FromRgb(59, 66, 82));
            }
        }
        return new SolidColorBrush(Color.FromRgb(37, 42, 57));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FilePathToBitmapConverter : IValueConverter
{
    public static readonly FilePathToBitmapConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string filePath || string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        if (!System.IO.File.Exists(filePath))
        {
            return null;
        }

        try
        {
            return new Bitmap(filePath);
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
