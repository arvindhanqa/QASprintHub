using QASprintHub.Models.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QASprintHub.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PRStatus status)
        {
            return status switch
            {
                PRStatus.Pending => new SolidColorBrush(Colors.Orange),
                PRStatus.Merged => new SolidColorBrush(Colors.Green),
                PRStatus.Blocked => new SolidColorBrush(Colors.Red),
                PRStatus.Declined => new SolidColorBrush(Colors.Gray),
                _ => new SolidColorBrush(Colors.Black)
            };
        }

        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
