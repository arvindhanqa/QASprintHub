using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QASprintHub.Converters;

public class StepToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return new SolidColorBrush(Colors.LightGray);

        if (int.TryParse(value.ToString(), out int currentStep) &&
            int.TryParse(parameter.ToString(), out int stepNumber))
        {
            // Current step is blue, completed steps are green, future steps are gray
            if (stepNumber == currentStep)
                return new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
            else if (stepNumber < currentStep)
                return new SolidColorBrush(Color.FromRgb(46, 204, 113)); // Green
            else
                return new SolidColorBrush(Colors.LightGray); // Gray
        }

        return new SolidColorBrush(Colors.LightGray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
