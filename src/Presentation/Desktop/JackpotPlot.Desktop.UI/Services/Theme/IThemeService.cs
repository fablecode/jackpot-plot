using Avalonia.Styling;

namespace JackpotPlot.Desktop.UI.Services.Theme;

public interface IThemeService
{
    ThemeVariant CurrentTheme { get; }

    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    void SetTheme(ThemeVariant theme);

    Task LoadSavedThemeAsync(CancellationToken cancellationToken = default);
}
