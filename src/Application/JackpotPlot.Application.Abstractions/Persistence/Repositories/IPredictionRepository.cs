using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Application.Abstractions.Persistence.Repositories;

public interface IPredictionRepository
{
    Task<PredictionDomain> Add(Guid? userId, PredictionResult predictionResult);
    Task<(Dictionary<int, int> hotNumbers, Dictionary<int, int> coldNumbers)> GetHotColdNumbersByLotteryId(int lotteryId);
    Task<Dictionary<int, int>> GetTrendingNumbers();
    Task<ImmutableDictionary<int, int>> GetPredictionSuccessRate();
    Task<NumberSpreadResult> GetNumberSpread();
    Task<ImmutableArray<LuckyPairResult>> GetLuckyPair();
}