namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Lucky number pair with frequency
/// </summary>
public record LuckyPairResult(
    int Number1,
    int Number2,
    long Frequency);
