using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Lottery.API.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddLotteryApiApplicationServices(this IServiceCollection services)
    {
        return services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
    }

}