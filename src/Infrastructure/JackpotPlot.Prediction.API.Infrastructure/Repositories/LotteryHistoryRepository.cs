using System.Collections.Immutable;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Prediction.API.Infrastructure.Databases;
using JackpotPlot.Prediction.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Prediction.API.Infrastructure.Repositories;

public sealed class LotteryHistoryRepository : ILotteryHistoryRepository
{
    private readonly IDbContextFactory<PredictionDbContext> _factory;

    public LotteryHistoryRepository(IDbContextFactory<PredictionDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<int> Add(LotteryDrawnEvent lotteryDrawnEvent)
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var lotteryHistory = new Lotteryhistory
            {
                Lotteryid = lotteryDrawnEvent.LotteryId,
                Drawdate = lotteryDrawnEvent.DrawDate,
                Winningnumbers = lotteryDrawnEvent.WinningNumbers.ToList(),
                Bonusnumbers = lotteryDrawnEvent.BonusNumbers.ToList(),
            };

            var addedLotteryHistory = await context.Lotteryhistories.AddAsync(lotteryHistory);

            await context.SaveChangesAsync();

            return addedLotteryHistory.Entity.Id;
        }
    }

    public async Task<ICollection<HistoricalDraw>> GetHistoricalDraws(int lotteryId)
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            return await context.Lotteryhistories
                .Where(draw => draw.Lotteryid == lotteryId)
                .OrderByDescending(draw => draw.Drawdate) // Sort by most recent first
                .Select(lh => new HistoricalDraw(lh.Id, lh.Lotteryid, lh.Drawdate, lh.Winningnumbers, lh.Bonusnumbers, lh.Createdat.Value))
                .ToListAsync();
        }
    }
}