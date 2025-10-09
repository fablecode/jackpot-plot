using JackpotPlot.Domain.Scheduling.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Lottery.API.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddLotteryApiApplicationServices(this IServiceCollection services)
    {
        return services
            .AddLotteryDomainServices()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
    }
    public static IServiceCollection AddLotteryDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<IDrawScheduleStrategy, DailyDrawStrategy>();
        services.AddSingleton<IDrawScheduleStrategy, IntervalDrawStrategy>();
        services.AddSingleton<IDrawScheduleStrategy, WeekdayDrawStrategy>();

        return services;
    }
}