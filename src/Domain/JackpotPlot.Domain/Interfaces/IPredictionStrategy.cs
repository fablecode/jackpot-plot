using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Interfaces;

public interface IPredictionStrategy
{
    Task<Result<PredictionResult>> Predict(int lotteryId);
    bool Handles(string strategy);
}