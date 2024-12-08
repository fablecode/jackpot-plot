using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Lottery.API.Application;
using JackpotPlot.Lottery.API.DatabaseMigration;
using JackpotPlot.Lottery.API.Infrastructure;
using Lottery.API.HostedServices;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var openApiInfo = new OpenApiInfo
    {
        Version = "v1",
        Title = "Lottery API",
        Description = "The core service for handling all lottery-related data. This includes CRUD (Create, Read, Update, Delete) operations for lotteries, draw events, results, and prize breakdowns.",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "License",
            Url = new Uri("https://example.com/license")
        }
    };

    options.SwaggerDoc("v1", openApiInfo);

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// IOptions<> configuration
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));

// Database Migration
builder.Services.AddLotteryApiDatabaseMigrationServices(builder.Configuration.GetConnectionString("LotteryApiDatabase"));

// Application Installer
builder.Services.AddLotteryApiApplicationServices();

// Infrastructure Installer
builder.Services.AddLotteryApiInfrastructureServices(builder.Configuration);

// Register the background service that will consume RabbitMQ messages
builder.Services.AddHostedService<LotteryResultsBackgroundService<Message<EurojackpotResult>>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(u =>
    {
        u.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "Lottery API V1");
    });
}

app.MapControllers();
app.UseRouting();

app.Run();
