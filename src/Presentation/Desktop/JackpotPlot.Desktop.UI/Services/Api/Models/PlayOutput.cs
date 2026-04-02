namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Represents a single play with predicted numbers
/// </summary>
public record PlayOutput(
    int PlayNumber,
    PredictionNumberOutput[] Predictions);
