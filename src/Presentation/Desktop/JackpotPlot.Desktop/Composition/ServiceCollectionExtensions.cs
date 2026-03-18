using JackpotPlot.Desktop.UI.Services.Navigation;
using JackpotPlot.Desktop.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using DashboardNavigationRequest = JackpotPlot.Desktop.UI.Services.Navigation.DashboardNavigationRequest;
using DrawHistoryNavigationRequest = JackpotPlot.Desktop.UI.Services.Navigation.DrawHistoryNavigationRequest;

namespace JackpotPlot.Desktop.Composition;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopApplication(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<INavigationMenuFactory, NavigationMenuFactory>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DrawHistoryViewModel>();

        services.AddSingleton<INavigationTarget>(
            new NavigationTarget<DashboardViewModel, DashboardNavigationRequest>(
                key: NavigationKeys.Dashboard,
                title: "Dashboard",
                requestFactory: () => new DashboardNavigationRequest(),
                iconKey: "Home",
                order: 0));

        services.AddSingleton<INavigationTarget>(
            new NavigationTarget<DrawHistoryViewModel, DrawHistoryNavigationRequest>(
                key: NavigationKeys.DrawHistory,
                title: "Draw History",
                requestFactory: () => new DrawHistoryNavigationRequest(),
                iconKey: "History",
                order: 1));

        services.AddSingleton<MainWindowViewModel>();

        return services;
    }
}