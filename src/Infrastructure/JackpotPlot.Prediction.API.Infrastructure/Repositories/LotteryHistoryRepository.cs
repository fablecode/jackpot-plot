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

    public async Task<(Dictionary<int, int> hotNumbers, Dictionary<int, int> coldNumbers)> GetAll()
    {
        using (var context = await _factory.CreateDbContextAsync())
        {
            var results = await context.Lotteryhistories.ToListAsync();

            // Count occurrences of all numbers
            var numberCounts = results.SelectMany(l => l.Winningnumbers)
                .GroupBy(n => n)
                .ToDictionary(g => g.Key, g => g.Count());

            // Find the 5 most common (hot) and 5 least common (cold) numbers
            var hotNumbers = numberCounts.OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);
            var coldNumbers = numberCounts.OrderBy(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);

            return (hotNumbers, coldNumbers);
        }
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