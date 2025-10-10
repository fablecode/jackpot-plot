using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Infrastructure;
using JackpotPlot.Infrastructure.Messaging;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            services.AddMassTransit(x =>
            {
                // Reuse your existing handler via the generic consumer
                x.AddConsumer<QueueToMediatorConsumer<Message<EurojackpotResult>>>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                    cfg.Host(settings.Host, h =>
                    {
                        h.Username(settings.Username);
                        h.Password(settings.Password);
                    });

                    // Mirror old queue name from LotteryResultsBackgroundService
                    cfg.ReceiveEndpoint("lottery-results", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.PrefetchCount = 1;
                        e.ConcurrentMessageLimit = 1;

                        // Bind to your existing topic exchange & keys (adjust keys to match your publisher)
                        e.Bind(settings.Exchange, configurator =>
                        {
                            configurator.ExchangeType = "topic";
                            configurator.RoutingKey = string.Join('.', RoutingKeys.LotteryResults, EventTypes.EurojackpotDraw);
                            configurator.Durable = true;
                        });

                        // Retries like your old ack/NACK flow (tune as needed)
                        e.UseMessageRetry(r => r.Immediate(3));

                        // Wire the consumer
                        e.ConfigureConsumer<QueueToMediatorConsumer<Message<EurojackpotResult>>>(context);
                    });
                });
            });

            return services;
        }
    }
}
