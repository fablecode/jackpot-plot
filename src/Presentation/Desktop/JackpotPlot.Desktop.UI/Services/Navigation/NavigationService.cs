using JackpotPlot.Desktop.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Desktop.UI.Services.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewModelBase> _history = new();

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ViewModelBase? CurrentViewModel { get; private set; }

    public bool CanGoBack => _history.Count > 0;

    public event EventHandler<NavigationChangedEventArgs>? Navigated;

    public async Task NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase
    {
        var next = _serviceProvider.GetRequiredService<TViewModel>();
        await NavigateInternalAsync(next, addToHistory: CurrentViewModel is not null, cancellationToken);
    }

    public async Task NavigateToAsync<TViewModel, TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase
        where TRequest : NavigationRequest
    {
        var next = _serviceProvider.GetRequiredService<TViewModel>();
        await NavigateInternalAsync(next, request, addToHistory: CurrentViewModel is not null, cancellationToken);
    }

    public async Task<bool> TryGoBackAsync(CancellationToken cancellationToken = default)
    {
        if (_history.Count == 0)
        {
            return false;
        }

        var previous = _history.Pop();
        var current = CurrentViewModel;

        if (current is INavigationAware currentAware)
        {
            await currentAware.OnNavigatedFromAsync(cancellationToken);
        }

        CurrentViewModel = previous;

        if (previous is INavigationAware previousAware)
        {
            await previousAware.OnNavigatedToAsync(cancellationToken);
        }

        Navigated?.Invoke(this, new NavigationChangedEventArgs(current, CurrentViewModel));
        return true;
    }

    private async Task NavigateInternalAsync(
        ViewModelBase nextViewModel,
        bool addToHistory,
        CancellationToken cancellationToken)
    {
        var previous = CurrentViewModel;

        if (previous is not null)
        {
            if (addToHistory)
            {
                _history.Push(previous);
            }

            if (previous is INavigationAware previousAware)
            {
                await previousAware.OnNavigatedFromAsync(cancellationToken);
            }
        }

        CurrentViewModel = nextViewModel;

        if (nextViewModel is INavigationAware nextAware)
        {
            await nextAware.OnNavigatedToAsync(cancellationToken);
        }

        Navigated?.Invoke(this, new NavigationChangedEventArgs(previous, CurrentViewModel));
    }

    private async Task NavigateInternalAsync<TRequest>(
        ViewModelBase nextViewModel,
        TRequest request,
        bool addToHistory,
        CancellationToken cancellationToken)
        where TRequest : NavigationRequest
    {
        var previous = CurrentViewModel;

        if (previous is not null)
        {
            if (addToHistory)
            {
                _history.Push(previous);
            }

            if (previous is INavigationAware previousAware)
            {
                await previousAware.OnNavigatedFromAsync(cancellationToken);
            }
        }

        CurrentViewModel = nextViewModel;

        if (nextViewModel is INavigationAware<TRequest> typedAware)
        {
            await typedAware.OnNavigatedToAsync(request, cancellationToken);
        }
        else if (nextViewModel is INavigationAware nextAware)
        {
            await nextAware.OnNavigatedToAsync(cancellationToken);
        }

        Navigated?.Invoke(this, new NavigationChangedEventArgs(previous, CurrentViewModel));
    }
}

public interface INavigationTarget
{
    string Key { get; }

    string Title { get; }

    string? IconKey { get; }

    int Order { get; }

    bool ShowInPrimaryMenu { get; }

    Task NavigateAsync(INavigationService navigationService, CancellationToken cancellationToken = default);
}

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

public interface INavigationMenuFactory
{
    IReadOnlyList<NavigationMenuItem> CreatePrimaryMenuItems();
}

public sealed class NavigationMenuFactory : INavigationMenuFactory
{
    private readonly IEnumerable<INavigationTarget> _navigationTargets;

    public NavigationMenuFactory(IEnumerable<INavigationTarget> navigationTargets)
    {
        _navigationTargets = navigationTargets;
    }

    public IReadOnlyList<NavigationMenuItem> CreatePrimaryMenuItems()
    {
        return _navigationTargets
            .Where(x => x.ShowInPrimaryMenu)
            .OrderBy(x => x.Order)
            .Select(x => new NavigationMenuItem
            {
                Key = x.Key,
                Title = x.Title,
                IconKey = x.IconKey,
                Target = x
            })
            .ToList();
    }
}