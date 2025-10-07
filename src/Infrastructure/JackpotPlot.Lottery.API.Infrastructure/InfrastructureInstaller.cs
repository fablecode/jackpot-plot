using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Infrastructure;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.HostedServices;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Lottery.API.Infrastructure
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddLotteryApiInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {

            return services
                .AddHostedServices()
                .AddMessagingServices()
                .AddDatabase(configuration)
                .AddRepositories();
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<ILotteryRepository, LotteryRepository>();
            services.AddTransient<IDrawRepository, DrawRepository>();
            services.AddTransient<IDrawResultRepository, DrawResultRepository>();
            services.AddTransient<ILotteryConfigurationRepository, LotteryConfigurationRepository>();
            services.AddTransient<ITicketRepository, TicketRepository>();
            services.AddTransient<ITicketPlayRepository, TicketPlayRepository>();

            return services;
        }
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<LotteryDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("LotteryApiDatabase")));

            return services;
        }
        public static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            // Register the background service that will consume RabbitMQ messages
            services.AddHostedService<LotteryResultsBackgroundService<Message<EurojackpotResult>>>();

            return services;
        }
    }
}
