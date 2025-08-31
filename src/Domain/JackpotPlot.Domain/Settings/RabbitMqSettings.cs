// RabbitMqSettings.cs
namespace JackpotPlot.Domain.Settings;

public record RabbitMqSettings
{
    public required string Host { get; init; }
    public int Port { get; init; }  // value type already non-nullable
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string Exchange { get; init; }
}