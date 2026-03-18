using JackpotPlot.Desktop.UI.Services.Navigation;

namespace JackpotPlot.Desktop.UI.Services;

public sealed class NavigationMenuItem
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public string? IconKey { get; init; }

    public required INavigationTarget Target { get; init; }
}