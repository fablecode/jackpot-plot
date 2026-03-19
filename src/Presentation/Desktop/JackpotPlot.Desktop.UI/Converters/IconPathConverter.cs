using Avalonia.Data.Converters;
using Avalonia.Media;
using JackpotPlot.Desktop.UI.Services;
using System.Globalization;

namespace JackpotPlot.Desktop.UI.Converters;

public sealed class IconPathConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string iconName && !string.IsNullOrEmpty(iconName))
        {
            var pathData = IconPaths.GetIconPath(iconName);
            return Geometry.Parse(pathData);
        }

        // Default icon (circle)
        return Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2Z");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
