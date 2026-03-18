using Avalonia.Data.Converters;
using System.Globalization;

namespace JackpotPlot.Desktop.UI.Converters;

public sealed class BoolToWidthConverter : IValueConverter
{
    public double CollapsedWidth { get; set; } = 60;
    public double ExpandedWidth { get; set; } = 220;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCollapsed)
        {
            return isCollapsed ? CollapsedWidth : ExpandedWidth;
        }

        return ExpandedWidth;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
