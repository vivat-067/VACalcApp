
using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace VACalcApp.Converters
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal percentage)
            {                                     
                return $"{percentage:0.00} %";
            }
            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                var cleaned = str.Replace("%", "").Replace(" ", "");
                cleaned = cleaned.Replace(',', '.');

                if (decimal.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }
            }
            return AvaloniaProperty.UnsetValue;
        }
    }
}