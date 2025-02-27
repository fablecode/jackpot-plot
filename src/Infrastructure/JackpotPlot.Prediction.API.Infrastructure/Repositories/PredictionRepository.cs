using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;
using JackpotPlot.Prediction.API.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Prediction.API.Infrastructure.Repositories;

public sealed class PredictionRepository : IPredictionRepository
{
    private readonly IDbContextFactory<PredictionDbContext> _factory;

    public PredictionRepository(IDbContextFactory<PredictionDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<PredictionDomain> Add(PredictionResult predictionResult)
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var newPrediction = new Models.Prediction
            {
                LotteryId = predictionResult.LotteryId,
                Strategy = predictionResult.Strategy,
                PredictedNumbers = predictionResult.PredictedNumbers.ToList(),
                BonusNumbers = predictionResult.BonusNumbers.ToList(),
                ConfidenceScore = (decimal?)predictionResult.ConfidenceScore
            };

            await context.Predictions.AddAsync(newPrediction);
            await context.SaveChangesAsync();

            return new PredictionDomain
            {
                Id = newPrediction.Id,
                LotteryId = newPrediction.LotteryId,
                Strategy = newPrediction.Strategy,
                PredictedNumbers = newPrediction.PredictedNumbers,
                BonusNumbers = newPrediction.BonusNumbers,
                ConfidenceScore = newPrediction.ConfidenceScore.GetValueOrDefault(),
                CreatedAt = newPrediction.CreatedAt.GetValueOrDefault()
            };
        }
    }

    public async Task<(Dictionary<int, int> hotNumbers, Dictionary<int, int> coldNumbers)> GetHotColdNumbersByLotteryId(int lotteryId)
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var predictions = await context.Predictions
                .Where(p => p.LotteryId == lotteryId)
                .ToListAsync();

            var numberCounts = predictions
                .SelectMany(p => p.PredictedNumbers)
                .GroupBy(n => n)
                .Select(g => new { Number = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var hotNumbers = numberCounts.Take(15).ToDictionary(x => x.Number, x => x.Count);
            var coldNumbers = numberCounts.TakeLast(15).ToDictionary(x => x.Number, x => x.Count);

            return new ValueTuple<Dictionary<int, int>, Dictionary<int, int>>(hotNumbers, coldNumbers );
        }
    }

    public async Task<Dictionary<int, int>> GetTrendingNumbers()
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var predictions = await context.Predictions.ToListAsync();

            var numberCounts = predictions
                .SelectMany(p => p.PredictedNumbers)
                .GroupBy(n => n)
                .OrderByDescending(g => g.Count())
                .Take(40) // ✅ Get top 40 trending numbers
                .ToDictionary(g => g.Key, g => g.Count());

            return numberCounts;
        }
    }
}

