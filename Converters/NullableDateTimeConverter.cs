using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VACalcApp.Converters
{
    public class NullableDateTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return date;
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return DateTime.Today; 
            return value;
        }
    }
}
