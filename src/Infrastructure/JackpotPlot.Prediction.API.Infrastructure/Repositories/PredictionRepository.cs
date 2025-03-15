using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Domain.ValueObjects;
using JackpotPlot.Prediction.API.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Immutable;
using System.Threading;

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

    public async Task<ImmutableDictionary<int, int>> GetPredictionSuccessRate()
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var predictions = await context.Predictions.ToListAsync();
            var winningNumbers = await context.Lotteryhistories.ToListAsync();

            var successRates = new Dictionary<int, int>();

            foreach (var prediction in predictions)
            {
                var draw = winningNumbers.FirstOrDefault(l => l.Lotteryid == prediction.LotteryId);
                if (draw != null)
                {
                    var matchCount = prediction.PredictedNumbers.Intersect(draw.Winningnumbers).Count();
                    if (!successRates.TryAdd(matchCount, 1))
                        successRates[matchCount]++;
                }
            }

            return successRates.ToImmutableDictionary(); // ✅ Returns aggregated histogram bins
        }
    }

    public async Task<NumberSpreadResult> GetNumberSpread()
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var predictions = await context.Predictions.ToListAsync();
            int low = 0, mid = 0, high = 0;

            foreach (var num in predictions.SelectMany(prediction => prediction.PredictedNumbers))
            {
                switch (num)
                {
                    case >= 1 and <= 20:
                        low++;
                        break;
                    case >= 21 and <= 40:
                        mid++;
                        break;
                    default:
                        high++;
                        break;
                }
            }

            return new NumberSpreadResult(low, mid, high);
        }
    }

    public async Task<ImmutableArray<LuckyPairResult>> GetLuckyPair()
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var results = new List<LuckyPairResult>();

            using (var conn = (NpgsqlConnection)context.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT * FROM get_frequent_prediction_pairs()", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new LuckyPairResult
                            (
                                reader.GetInt32(0),
                                reader.GetInt32(1), 
                                reader.GetInt64(2)
                            ));
                        }
                    }
                }
            }

            return [..results];
        }
    }

}

