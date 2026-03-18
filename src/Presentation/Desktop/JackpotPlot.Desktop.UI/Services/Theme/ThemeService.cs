using Avalonia;
using Avalonia.Styling;
using System.Text.Json;

namespace JackpotPlot.Desktop.UI.Services.Theme;

public sealed class ThemeService : IThemeService
{
    private const string ThemePreferenceFileName = "theme-preference.json";
    private static readonly string ThemePreferencePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "JackpotPlot",
        ThemePreferenceFileName);

    private readonly Application _application;
    private ThemeVariant _currentTheme;

    public ThemeService()
    {
        _application = Application.Current ?? throw new InvalidOperationException("Application.Current is null");
        _currentTheme = ThemeVariant.Default;
    }

    public ThemeVariant CurrentTheme => _currentTheme;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public void SetTheme(ThemeVariant theme)
    {
        var previousTheme = _currentTheme;
        _currentTheme = theme;
        _application.RequestedThemeVariant = theme;

        SaveThemePreference(theme);

        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(previousTheme, theme));
    }

    public async Task LoadSavedThemeAsync(CancellationToken cancellationToken = default)
    {
        var savedTheme = await GetSavedThemePreferenceAsync(cancellationToken);
        SetTheme(savedTheme);
    }

    private void SaveThemePreference(ThemeVariant theme)
    {
        try
        {
            var directory = Path.GetDirectoryName(ThemePreferencePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var preference = new ThemePreference { Theme = theme.Key.ToString() };
            var json = JsonSerializer.Serialize(preference);
            File.WriteAllText(ThemePreferencePath, json);
        }
        catch
        {
        }
    }

    private async Task<ThemeVariant> GetSavedThemePreferenceAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(ThemePreferencePath))
            {
                return ThemeVariant.Default;
            }

            var json = await File.ReadAllTextAsync(ThemePreferencePath, cancellationToken);
            var preference = JsonSerializer.Deserialize<ThemePreference>(json);

            return preference?.Theme switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
        }
        catch
        {
            return ThemeVariant.Default;
        }
    }

    private sealed class ThemePreference
    {
        public string Theme { get; set; } = string.Empty;
    }
}
