namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Represents a predicted number with its metadata
/// </summary>
public record PredictionNumberOutput(
    int Number,
    int Frequency,
    string Status,  // "hot", "cold", "neutral"
    string NumberType); // "main", "bonus"
