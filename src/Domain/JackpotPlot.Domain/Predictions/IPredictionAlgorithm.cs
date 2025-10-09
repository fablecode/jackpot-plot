using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Predictions;

public interface IPredictionAlgorithm
{
    PredictionResult Predict(LotteryConfigurationDomain config, IReadOnlyList<HistoricalDraw> history, Random random);
}