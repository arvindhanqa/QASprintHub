using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QASprintHub.Converters;

public class GreaterThanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Visibility.Collapsed;

        if (int.TryParse(value.ToString(), out int intValue) &&
            int.TryParse(parameter.ToString(), out int paramValue))
        {
            return intValue > paramValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
