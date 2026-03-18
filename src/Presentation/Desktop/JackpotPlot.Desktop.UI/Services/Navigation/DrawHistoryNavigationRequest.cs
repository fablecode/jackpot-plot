namespace JackpotPlot.Desktop.UI.Services.Navigation;

public sealed record DrawHistoryNavigationRequest(
    string? SearchText = null,
    DateOnly? From = null,
    DateOnly? To = null) : NavigationRequest;