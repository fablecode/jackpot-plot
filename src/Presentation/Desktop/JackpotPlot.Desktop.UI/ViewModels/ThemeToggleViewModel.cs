using Avalonia.Styling;
using JackpotPlot.Desktop.UI.Services.Theme;

namespace JackpotPlot.Desktop.UI.ViewModels;

public sealed class ThemeToggleViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;

    public ThemeToggleViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        _themeService.ThemeChanged += OnThemeChanged;
    }

    public bool IsDarkTheme => _themeService.CurrentTheme == ThemeVariant.Dark;

    public void ToggleTheme()
    {
        var newTheme = _themeService.CurrentTheme == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;

        _themeService.SetTheme(newTheme);
    }

    public void SetLightTheme() => _themeService.SetTheme(ThemeVariant.Light);

    public void SetDarkTheme() => _themeService.SetTheme(ThemeVariant.Dark);

    public void SetSystemTheme() => _themeService.SetTheme(ThemeVariant.Default);

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsDarkTheme));
    }
}
