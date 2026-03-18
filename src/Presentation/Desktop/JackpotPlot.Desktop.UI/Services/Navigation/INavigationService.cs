using JackpotPlot.Desktop.UI.ViewModels;

namespace JackpotPlot.Desktop.UI.Services.Navigation;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }

    bool CanGoBack { get; }

    event EventHandler<NavigationChangedEventArgs>? Navigated;

    Task NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase;

    Task NavigateToAsync<TViewModel, TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase
        where TRequest : NavigationRequest;

    Task<bool> TryGoBackAsync(CancellationToken cancellationToken = default);
}