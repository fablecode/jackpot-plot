// ApiSettings.cs
namespace JackpotPlot.Domain.Settings;

public record ApiSettings
{
    public required string LotteryServiceUrl { get; init; }
}