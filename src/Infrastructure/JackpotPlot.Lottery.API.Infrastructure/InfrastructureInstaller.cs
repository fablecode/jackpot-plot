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
using Newtonsoft.Json;

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
                x.AddConsumer<QueueToMediatorConsumer<Message<EurojackpotResult>>>();

                // Optional: global endpoint naming for auto-mapped endpoints elsewhere
                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseNewtonsoftJsonSerializer();
                    cfg.ConfigureNewtonsoftJsonSerializer(s =>
                    {
                        s.NullValueHandling = NullValueHandling.Ignore;
                        s.DateParseHandling = DateParseHandling.DateTimeOffset;
                        return s;
                    });
                    cfg.ConfigureNewtonsoftJsonDeserializer(s =>
                    {
                        s.NullValueHandling = NullValueHandling.Ignore;
                        return s;
                    });

                    var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                    cfg.Host(settings.Host, h =>
                    {
                        h.Username(settings.Username);
                        h.Password(settings.Password);
                    });

                    // Avoid per-message exchanges from this process
                    cfg.Publish<Message<EurojackpotResult>>(topologyConfigurator => topologyConfigurator.Exclude = true);

                    cfg.ReceiveEndpoint("lottery-results", e =>
                    {
                        e.ConfigureConsumeTopology = false;

                        // HA / performance knobs
                        e.PrefetchCount = 1;               // tune
                        e.ConcurrentMessageLimit = 1;       // tune

                        // Bind to your topic exchange with explicit keys
                        e.Bind(settings.Exchange, b =>
                        {
                            b.ExchangeType = "topic";
                            b.RoutingKey = $"{RoutingKeys.LotteryResults}.{EventTypes.EurojackpotDraw}";
                            b.Durable = true;
                        });

                        // Resiliency
                        e.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3)));

                        // Consumer
                        e.ConfigureConsumer<QueueToMediatorConsumer<Message<EurojackpotResult>>>(context);
                    });
                });
            });

            return services;
        }
    }
}
