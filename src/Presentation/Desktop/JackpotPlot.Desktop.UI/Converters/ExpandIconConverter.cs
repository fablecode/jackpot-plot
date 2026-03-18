using Avalonia.Data.Converters;
using System.Globalization;

namespace JackpotPlot.Desktop.UI.Converters;

public sealed class ExpandIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? "−" : "+";
        }

        return "+";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
