﻿using Coravel;
using JackpotPlot.Application;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Infrastructure;
using LotteryDataCollector.Service.Jobs.Eurojackpot;
using Serilog;

namespace LotteryDataCollector.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = CreateHostBuilder(args);

            var host = builder.Build();

            // Fetch Eurojackpot History once on startup
            var historyJob = host.Services.GetRequiredService<FetchEurojackpotDrawHistoryJob>();
            await historyJob.Invoke();  // Run the history job immediately when the app starts

            // Schedule the job to run on Tuesday and Friday at 8:00 PM CET/CEST (local time)
            //host.Services.UseScheduler(scheduler =>
            //{
            //    scheduler.Schedule<FetchEurojackpotResultsJob>()
            //        .At(20, 0)  // 8:00 PM local time (CET/CEST)
            //        .On(DayOfWeek.Tuesday)
            //        .On(DayOfWeek.Friday)
            //        .When(runner =>
            //        {
            //            // Use NodaTime to check the current local time in CET/CEST
            //            var systemClock = SystemClock.Instance;
            //            var now = systemClock.GetCurrentInstant();
            //            var zone = DateTimeZoneProviders.Tzdb["Europe/Brussels"];  // Eurojackpot draw is in Brussels
            //            var localDateTime = now.InZone(zone).LocalDateTime;

            //            // Only run if the time is 8:00 PM in local time (CET/CEST)
            //            return localDateTime.Hour == 20 && (localDateTime.DayOfWeek == IsoDayOfWeek.Tuesday || localDateTime.DayOfWeek == IsoDayOfWeek.Friday);
            //        });
            //});

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLogging();

                //Configuration settings
                services.Configure<RabbitMqSettings>(context.Configuration.GetRequiredSection(nameof(RabbitMqSettings)));

                // Register services

                // Register Coravel jobs
                services.AddTransient<FetchEurojackpotDrawHistoryJob>();

                // Register Coravel
                services.AddScheduler();

                // Application Installer
                services.AddApplicationServices();

                // Infrastructure Installer
                services.AddInfrastructureServices();

            }).UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));
    }
}