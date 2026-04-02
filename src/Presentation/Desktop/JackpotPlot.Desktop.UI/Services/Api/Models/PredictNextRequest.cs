namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Request model for generating predictions
/// </summary>
public record PredictNextRequest(
    int LotteryId,
    int NumberOfPlays = 5,
    string Strategy = "random",
    Guid? UserId = null);
