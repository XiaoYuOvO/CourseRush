using System;
using System.Globalization;
using System.Windows.Data;
using CourseRush.Core.Util;

namespace CourseRush.Util;

public class TranslatableTextToStringConverter : IValueConverter 
{
    public static readonly TranslatableTextToStringConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TranslatableText translatableText) return value;
        return translatableText.Translate(Language.ResourceManager, parameter);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}