using System.Reflection;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Prediction.API.Application;
using JackpotPlot.Prediction.API.DatabaseMigration;
using JackpotPlot.Prediction.API.Infrastructure;
using Microsoft.OpenApi.Models;
using Prediction.API.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var openApiInfo = new OpenApiInfo
    {
        Version = "v1",
        Title = "Prediction API",
        Description = "This service houses prediction logic and analytics based on historical data, user preferences, and statistics to recommend numbers for upcoming draws.",
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
builder.Services.AddPredictionApiDatabaseMigrationServices(builder.Configuration.GetConnectionString("PredictionApiDatabase"));

// Application Installer
builder.Services.AddPredictionApiApplicationServices();

// Infrastructure Installer
builder.Services.AddPredictionApiInfrastructureServices(builder.Configuration);

// Register the background service that will consume RabbitMQ messages
builder.Services.AddHostedService<LotteryDrawnBackgroundService<Message<LotteryDrawnEvent>>>();

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
        c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "Prediction API V1");
    });
}

app.MapControllers();
app.UseRouting();

app.Run();
