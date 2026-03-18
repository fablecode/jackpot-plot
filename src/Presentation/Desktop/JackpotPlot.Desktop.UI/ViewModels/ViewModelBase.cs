using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JackpotPlot.Desktop.UI.Services.Navigation;
using System.Collections.ObjectModel;

namespace JackpotPlot.Desktop.UI.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
}

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private NavigationMenuItem? _selectedNavigationItem;

    public MainWindowViewModel()
    {
        Title = "JackpotPlot Desktop";
        NavigationItems = new ObservableCollection<NavigationMenuItem>();
        CurrentPage = new DashboardViewModel();

        NavigateCommand = new AsyncRelayCommand<NavigationMenuItem>(NavigateAsync);
        GoBackCommand = new AsyncRelayCommand(GoBackAsync);
    }

    public MainWindowViewModel(
        INavigationService navigationService,
        INavigationMenuFactory navigationMenuFactory)
    {
        _navigationService = navigationService;
        Title = "JackpotPlot Desktop";

        NavigationItems = new ObservableCollection<NavigationMenuItem>(navigationMenuFactory.CreatePrimaryMenuItems());

        CurrentPage = _navigationService.CurrentViewModel;

        NavigateCommand = new AsyncRelayCommand<NavigationMenuItem>(NavigateAsync);
        GoBackCommand = new AsyncRelayCommand(GoBackAsync, CanGoBack);

        _navigationService.Navigated += OnNavigated;
        UpdateSelectedNavigationItem();
    }

    public string Title { get; }

    public ObservableCollection<NavigationMenuItem> NavigationItems { get; }

    public IAsyncRelayCommand<NavigationMenuItem> NavigateCommand { get; }

    public IAsyncRelayCommand GoBackCommand { get; }

    private async Task NavigateAsync(NavigationMenuItem? item)
    {
        if (item is null || _navigationService is null)
        {
            return;
        }

        await item.Target.NavigateAsync(_navigationService);
    }

    private bool CanGoBack()
    {
        return _navigationService?.CanGoBack ?? false;
    }

    private async Task GoBackAsync()
    {
        if (_navigationService is not null)
        {
            await _navigationService.TryGoBackAsync();
            GoBackCommand.NotifyCanExecuteChanged();
        }
    }

    private void OnNavigated(object? sender, NavigationChangedEventArgs e)
    {
        CurrentPage = e.CurrentViewModel;
        GoBackCommand.NotifyCanExecuteChanged();
        UpdateSelectedNavigationItem();
    }

    private void UpdateSelectedNavigationItem()
    {
        var key = (CurrentPage as IHasNavigationKey)?.NavigationKey;

        SelectedNavigationItem = key is null
            ? null
            : NavigationItems.FirstOrDefault(x =>
                x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed partial class DashboardViewModel : ViewModelBase, INavigationAware<NavigationRequest>, IHasNavigationKey
{
    public string NavigationKey => NavigationKeys.Dashboard;

    public string Title => "Dashboard";

    public string Summary =>
        "Overview of latest draws, prediction activity, and service status.";

    public Task OnNavigatedToAsync(NavigationRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public sealed class NavigationMenuItem
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public string? IconKey { get; init; }

    public required INavigationTarget Target { get; init; }
}

public abstract record NavigationRequest;

public sealed record DashboardNavigationRequest() : NavigationRequest;