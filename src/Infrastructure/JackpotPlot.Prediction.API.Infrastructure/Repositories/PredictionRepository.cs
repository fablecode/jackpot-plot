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
}