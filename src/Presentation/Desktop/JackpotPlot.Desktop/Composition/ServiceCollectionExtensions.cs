using JackpotPlot.Desktop.UI.Services.Navigation;
using JackpotPlot.Desktop.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using DashboardNavigationRequest = JackpotPlot.Desktop.UI.Services.Navigation.DashboardNavigationRequest;

namespace JackpotPlot.Desktop.Composition;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopApplication(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<INavigationMenuFactory, NavigationMenuFactory>();

        services.AddTransient<DashboardViewModel>();

        services.AddSingleton<INavigationTarget>(
            new NavigationTarget<DashboardViewModel, DashboardNavigationRequest>(
                key: NavigationKeys.Dashboard,
                title: "Dashboard",
                requestFactory: () => new DashboardNavigationRequest(),
                iconKey: "Home",
                order: 0));

        services.AddSingleton<MainWindowViewModel>();

        return services;
    }
}