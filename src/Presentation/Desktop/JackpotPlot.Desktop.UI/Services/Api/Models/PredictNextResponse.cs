namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Response model containing generated predictions
/// </summary>
public record PredictNextResponse(
    int LotteryId,
    int NumberOfPlays,
    string Strategy,
    PlayOutput[] Plays);
