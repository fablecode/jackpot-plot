using Avalonia.Styling;

namespace JackpotPlot.Desktop.UI.Services.Theme;

public sealed class ThemeChangedEventArgs : EventArgs
{
    public ThemeChangedEventArgs(
        ThemeVariant previousTheme,
        ThemeVariant currentTheme)
    {
        PreviousTheme = previousTheme;
        CurrentTheme = currentTheme;
    }

    public ThemeVariant PreviousTheme { get; }

    public ThemeVariant CurrentTheme { get; }
}
