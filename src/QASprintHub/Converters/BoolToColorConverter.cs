using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QASprintHub.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isToday && isToday)
        {
            return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // #E74C3C (red for today)
        }
        return new SolidColorBrush(Color.FromRgb(44, 62, 80)); // #2C3E50 (dark blue)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
