using Coravel;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Infrastructure;
using LotteryDataCollector.Service.Infrastructure;
using LotteryDataCollector.Service.Jobs.Eurojackpot;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NodaTime;
using Serilog;

namespace LotteryDataCollector.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = CreateHostBuilder(args);

                var host = builder.Build();

                // Fetch Eurojackpot History once on startup
                var historyJob = host.Services.GetRequiredService<FetchEurojackpotDrawHistoryJob>();
                await historyJob.Invoke();  // Run the history job immediately when the app starts

                // Schedule the job to run on Tuesday and Friday at 8:00 PM CET/CEST (local time)
                host.Services.UseScheduler(scheduler =>
                {
                    scheduler.Schedule<FetchEurojackpotResultsJob>()
                        .DailyAt(20, 0)  // 8:00 PM local time (CET/CEST)
                        .Tuesday()
                        .Friday()
                        .When(() =>
                        {
                            // Use NodaTime to check the current local time in CET/CEST
                            var systemClock = SystemClock.Instance;
                            var now = systemClock.GetCurrentInstant();
                            var zone = DateTimeZoneProviders.Tzdb["Europe/Brussels"];  // Eurojackpot draw is in Brussels
                            var localDateTime = now.InZone(zone).LocalDateTime;

                            // Only run if the time is 8:00 PM in local time (CET/CEST)
                            return Task.FromResult(localDateTime.Hour == 20 && (localDateTime.DayOfWeek == IsoDayOfWeek.Tuesday || localDateTime.DayOfWeek == IsoDayOfWeek.Friday));
                        });
                })
                .OnError((exception) =>
                    Log.Logger.Error(exception, "FetchEurojackpotResults job exception")
                );

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "LotteryDataCollector.Service fatal error");
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddUserSecrets<Program>();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging();

                //Configuration settings
                services.Configure<RabbitMqSettings>(context.Configuration.GetRequiredSection(nameof(RabbitMqSettings)));

                // Register services
                // --- MassTransit (publisher-first) ---
                services.AddMassTransit(x =>
                {
                    // Consistent kebab naming
                    x.SetKebabCaseEndpointNameFormatter();

                    // No consumers yet — this service publishes only (for now)

                    x.UsingRabbitMq((ctx, cfg) =>
                    {
                        // Make Newtonsoft the default serializer
                        cfg.UseNewtonsoftJsonSerializer();

                        // Optional: customize serializer settings
                        cfg.ConfigureNewtonsoftJsonSerializer(settings =>
                        {
                            settings.NullValueHandling = NullValueHandling.Ignore;
                            settings.DateParseHandling = DateParseHandling.DateTimeOffset;
                            // settings.TypeNameHandling = TypeNameHandling.None; // usually keep this off
                            return settings;
                        });

                        // Optional: customize deserializer settings (can differ)
                        cfg.ConfigureNewtonsoftJsonDeserializer(settings =>
                        {
                            settings.NullValueHandling = NullValueHandling.Ignore;
                            return settings;
                        });

                        var opts = ctx.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
                        cfg.Host(opts.Host, "/", h =>
                        {
                            h.Username(opts.Username);
                            h.Password(opts.Password);
                        });

                        // Optional: basic health + diag
                        cfg.AutoStart = true;

                        // (Optional) health, diagnostics, concurrency tuning
                        cfg.PrefetchCount = (ushort)Environment.ProcessorCount;

                    });
                });

                // Register Coravel jobs
                services.AddTransient<FetchEurojackpotDrawHistoryJob>();

                // Register Coravel
                services.AddScheduler();

                // Application Installer
                services.AddLotteryDataCollectorServiceApplicationServices();

                // Infrastructure Installer
                services.AddLotteryDataCollectorServiceInfrastructureServices();

            }).UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));
    }
}
