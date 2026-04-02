namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Lottery configuration details
/// </summary>
public record LotteryConfigurationDto(
    int Id,
    int LotteryId,
    int MainNumberCount,
    int MainNumberMin,
    int MainNumberMax,
    int? BonusNumberCount = null,
    int? BonusNumberMin = null,
    int? BonusNumberMax = null);
