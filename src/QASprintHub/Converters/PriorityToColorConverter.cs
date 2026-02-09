using QASprintHub.Models.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QASprintHub.Converters;

public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PRPriority priority)
        {
            return priority switch
            {
                PRPriority.Low => new SolidColorBrush(Colors.Gray),
                PRPriority.Normal => new SolidColorBrush(Colors.Black),
                PRPriority.High => new SolidColorBrush(Colors.Orange),
                PRPriority.Critical => new SolidColorBrush(Colors.Red),
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
