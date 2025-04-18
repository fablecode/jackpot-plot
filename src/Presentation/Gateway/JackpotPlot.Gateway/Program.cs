using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy for Ocelot
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://angular-client:4200") // Allow Angular app
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // If using authentication cookies
        });
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("KeycloakJWT", options =>
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