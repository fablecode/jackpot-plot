using Avalonia.Data.Converters;
using System.Globalization;

namespace JackpotPlot.Desktop.UI.Converters;

public sealed class BoolToAngleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCollapsed)
        {
            return isCollapsed ? 180.0 : 0.0;
        }

        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
