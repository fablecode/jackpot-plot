namespace JackpotPlot.Domain.Settings;

public record RabbitMqSettings
{
    public string Host { get; init; }
    public int Port { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public string Exchange { get; init; }
}
