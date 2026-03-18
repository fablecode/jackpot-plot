namespace JackpotPlot.Desktop.UI.Services.Navigation;

public interface INavigationTarget
{
    string Key { get; }

    string Title { get; }

    string? IconKey { get; }

    int Order { get; }

    bool ShowInPrimaryMenu { get; }

    Task NavigateAsync(INavigationService navigationService, CancellationToken cancellationToken = default);
}