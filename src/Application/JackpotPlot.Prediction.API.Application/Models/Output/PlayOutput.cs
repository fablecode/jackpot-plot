namespace JackpotPlot.Prediction.API.Application.Models.Output;

public record PlayOutput(int PlayNumber, PredictionNumberOutput[] Predictions);