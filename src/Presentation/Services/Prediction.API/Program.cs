using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Prediction.API.Application;
using JackpotPlot.Prediction.API.DatabaseMigration;
using JackpotPlot.Prediction.API.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using JackpotPlot.Prediction.API.Infrastructure.HostedServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add configuration for Serilog from appsettings.json
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services) // optional but recommended for DI context
        .Enrich.WithExceptionDetails() // still needed here for Serilog.Exceptions to hook properly
        .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
        .Enrich.WithProperty("Environment", context.HostingEnvironment);
});

// Global exception handlers
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Log.Fatal(e.ExceptionObject as Exception, "An unhandled exception occurred.");
    Log.CloseAndFlush();
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    Log.Error(e.Exception, "An unobserved task exception occurred.");
    e.SetObserved();
};


try
{
    // ─────────────────────────────────────────────────────
    // 1. Controllers & Swagger
    // ─────────────────────────────────────────────────────
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

        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });

    // ─────────────────────────────────────────────────────
    // 2. ✅ Authentication Fix (default scheme set explicitly)
    // ─────────────────────────────────────────────────────
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = "http://localhost:8085/realms/jackpotplot";
            options.RequireHttpsMetadata = false;
            options.Audience = "account";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "http://localhost:8085/realms/jackpotplot",

                ValidateAudience = true,
                ValidAudience = "account",

                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,

                // ✅ Correct way to bypass signature
                SignatureValidator = (token, parameters) =>
                {
                    var handler = new JsonWebTokenHandler();
                    var result = handler.ReadJsonWebToken(token); // ✅ returns JsonWebToken
                    return result;
                }
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"JWT auth failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("JWT token validated (signature skipped)");
                    return Task.CompletedTask;
                }
            };
        });


    builder.Services.AddAuthorization();

    // ─────────────────────────────────────────────────────
    // 3. Custom Configuration & Services
    // ─────────────────────────────────────────────────────
    builder.Services.AddRouting(options => options.LowercaseUrls = true);
    builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));
    builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(nameof(ApiSettings)));

    builder.Services.AddPredictionApiDatabaseMigrationServices(builder.Configuration.GetConnectionString("PredictionApiDatabase"));
    builder.Services.AddPredictionApiApplicationServices();
    builder.Services.AddPredictionApiInfrastructureServices(builder.Configuration);

    // ─────────────────────────────────────────────────────
    // 4. Build & Run Pipeline
    // ─────────────────────────────────────────────────────
    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagContext, httpContext) =>
        {
            diagContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    app.UseMiddleware<ExceptionHandlingMiddleware>();


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

    app.UseRouting();

    // 🧠 Most important: Middleware order
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}


// Middleware class
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught by middleware.");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected error occurred.");
        }
    }
}
