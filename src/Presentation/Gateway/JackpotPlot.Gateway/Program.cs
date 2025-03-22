using System.Diagnostics;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy for Ocelot
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Allow Angular app
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // If using authentication cookies
        });
});

builder.Services.AddAuthentication()
    .AddJwtBearer("KeycloakJWT", options =>
    {
        options.Authority = "http://keycloak:8080/realms/jackpotplot";
        options.Audience = "account";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidIssuer = "http://keycloak:8080/realms/jackpotplot";
    });


// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAngular"); // Apply CORS middleware BEFORE Ocelot

// Use Ocelot middleware
app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();