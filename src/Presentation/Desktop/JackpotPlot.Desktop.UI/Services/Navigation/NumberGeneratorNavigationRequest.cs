namespace JackpotPlot.Desktop.UI.Services.Navigation;

/// <summary>
/// Navigation request for the Number Generator view
/// </summary>
public sealed record NumberGeneratorNavigationRequest(
    int? PreSelectedLotteryId = null,
    string? PreSelectedStrategy = null) : NavigationRequest;
