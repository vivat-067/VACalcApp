using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using VACalcApp.Services;

namespace VACalcApp.Converters
{
    public class InterestCalculationMethodToBoolConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InterestCalculationMethod methodValue &&
                parameter is string strParam)
            {
                if (Enum.TryParse<InterestCalculationMethod>(strParam, out var parsedMethod))
                {
                    return methodValue == parsedMethod;
                }
            }

            return false;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string strParam &&
                Enum.TryParse<InterestCalculationMethod>(strParam, out var parsedMethod))
            {
                return parsedMethod;
            }

            return AvaloniaProperty.UnsetValue;
        }
    }
}