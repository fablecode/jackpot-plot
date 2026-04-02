namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Number spread analysis (distribution across ranges)
/// </summary>
public record NumberSpreadResult(
    int Low,
    int Mid,
    int High);
