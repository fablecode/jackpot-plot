namespace JackpotPlot.Prediction.API.Application.Models.Output;

public record PredictionOutput(int PlayNumber, PredictionNumberOutput[] Predictions);