using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.ValueObjects;
using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Repositories;

public interface IPredictionRepository
{
    Task<PredictionDomain> Add(PredictionResult predictionResult);
    Task<(Dictionary<int, int> hotNumbers, Dictionary<int, int> coldNumbers)> GetHotColdNumbersByLotteryId(int lotteryId);
    Task<Dictionary<int, int>> GetTrendingNumbers();
    Task<ImmutableDictionary<int, int>> GetPredictionSuccessRate();
    Task<NumberSpreadResult> GetNumberSpread();
}