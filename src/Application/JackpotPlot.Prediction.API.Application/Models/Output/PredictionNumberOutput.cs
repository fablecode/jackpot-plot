namespace JackpotPlot.Prediction.API.Application.Models.Output;

public record PredictionNumberOutput(int Number, int Frequency, string Status, string NumberType);