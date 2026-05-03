using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
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
