using JackpotPlot.Desktop.UI.Services.Api;
using JackpotPlot.Desktop.UI.Services.Menu;
using JackpotPlot.Desktop.UI.Services.Navigation;
using JackpotPlot.Desktop.UI.Services.Theme;
using JackpotPlot.Desktop.UI.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;
using DashboardNavigationRequest = JackpotPlot.Desktop.UI.Services.Navigation.DashboardNavigationRequest;
using DrawHistoryNavigationRequest = JackpotPlot.Desktop.UI.Services.Navigation.DrawHistoryNavigationRequest;

namespace JackpotPlot.Desktop.Composition;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApiClients(configuration)
            .AddViewModels()
            .AddServices()
            .AddNavigationTargets();

        return services;
    }

    private static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                })
        };

        // Register Predictions API Client
        var predictionsApiUrl = configuration["ApiSettings:PredictionServiceUrl"] 
            ?? "https://localhost:5001"; // Fallback URL

        services.AddRefitClient<IPredictionsApiClient>(refitSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(predictionsApiUrl);
                c.Timeout = TimeSpan.FromSeconds(30);
            });

        // Register Lottery API Client
        var lotteryApiUrl = configuration["ApiSettings:LotteryServiceUrl"] 
            ?? "https://localhost:5002"; // Fallback URL

        services.AddRefitClient<ILotteryApiClient>(refitSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(lotteryApiUrl);
                c.Timeout = TimeSpan.FromSeconds(30);
            });

        return services;
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<UI.Views.MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<SidebarViewModel>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DrawHistoryViewModel>();
        services.AddTransient<NumberGeneratorViewModel>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<INavigationMenuFactory, NavigationMenuFactory>();
        services.AddSingleton<IMenuService, MenuService>();

        return services;
    }

    private static IServiceCollection AddNavigationTargets(this IServiceCollection services)
    {
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

        services.AddSingleton<INavigationTarget>(
            new NavigationTarget<NumberGeneratorViewModel, NumberGeneratorNavigationRequest>(
                key: NavigationKeys.NumberGenerator,
                title: "Number Generator",
                requestFactory: () => new NumberGeneratorNavigationRequest(),
                iconKey: "AutoFix",
                order: 2));

        return services;
    }
}