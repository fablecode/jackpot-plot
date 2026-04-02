namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Lottery information
/// </summary>
public record LotteryDto(
    int Id,
    string Name,
    DateTime? DrawDate = null,
    decimal? Jackpot = null,
    int[]? NumbersDrawn = null);
