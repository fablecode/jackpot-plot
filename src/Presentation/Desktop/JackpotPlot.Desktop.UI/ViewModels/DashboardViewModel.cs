using JackpotPlot.Desktop.UI.Services.Navigation;

namespace JackpotPlot.Desktop.UI.ViewModels;

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