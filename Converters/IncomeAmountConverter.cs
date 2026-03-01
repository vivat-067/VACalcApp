using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace VACalcApp.Converters
{
    internal class IncomeAmountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                if (amount == 0m)   // Если значение равно 0, возвращаем пустую строку
                {
                    return string.Empty;
                }

                return amount.ToString("C2", culture ?? CultureInfo.CurrentCulture);
            }


            // Для всех остальных случаев возвращаем пустую строку
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("IncomeAmountConverter cannot convert back.");
        }

    }

}