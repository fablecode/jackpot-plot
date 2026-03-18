using System;
using Avalonia;
using JackpotPlot.Desktop.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Desktop.Hosting;

public static class DesktopHostBuilder
{
    public static AppBuilder CreateAppBuilder()
    {
        return AppBuilder.Configure<App>();
    }

    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDesktopApplication();
    }
}