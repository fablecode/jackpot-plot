using JackpotPlot.Desktop.UI.ViewModels;

namespace JackpotPlot.Desktop.UI.Services.Navigation;

public sealed class NavigationTarget<TViewModel, TRequest> : INavigationTarget
    where TViewModel : ViewModelBase
    where TRequest : NavigationRequest
{
    private readonly Func<TRequest> _requestFactory;

    public NavigationTarget(
        string key,
        string title,
        Func<TRequest> requestFactory,
        string? iconKey = null,
        int order = 0,
        bool showInPrimaryMenu = true)
    {
        Key = key;
        Title = title;
        IconKey = iconKey;
        Order = order;
        ShowInPrimaryMenu = showInPrimaryMenu;
        _requestFactory = requestFactory;
    }

    public string Key { get; }

    public string Title { get; }

    public string? IconKey { get; }

    public int Order { get; }

    public bool ShowInPrimaryMenu { get; }

    public Task NavigateAsync(INavigationService navigationService, CancellationToken cancellationToken = default)
    {
        return navigationService.NavigateToAsync<TViewModel, TRequest>(_requestFactory(), cancellationToken);
    }
}