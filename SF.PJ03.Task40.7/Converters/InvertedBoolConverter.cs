using System.Globalization;

namespace SF.PJ03.Task40._7_.Converters;

/// <summary>
/// Конвертер значений, который инвертирует логическое значение (true в false, false в true). 
/// Используется в привязках данных XAML.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    // Инвертирует логическое значение.
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return value;   // Возвращаем исходное значение, если это не bool
    }

    // Инвертирует логическое значение обратно (для двусторонних привязок).
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return value;   // Возвращаем исходное значение, если это не bool
    }
}