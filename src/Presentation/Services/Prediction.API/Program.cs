using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Settings;
using JackpotPlot.Prediction.API.Application;
using JackpotPlot.Prediction.API.DatabaseMigration;
using JackpotPlot.Prediction.API.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Prediction.API.HostedServices;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddHostedService<LotteryDrawnBackgroundService<Message<LotteryDrawnEvent>>>();

// ─────────────────────────────────────────────────────
// 4. Build & Run Pipeline
// ─────────────────────────────────────────────────────
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

app.UseRouting();

// 🧠 Most important: Middleware order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
