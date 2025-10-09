using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public sealed class LotteryConfigurationRepository : ILotteryConfigurationRepository
{
    private readonly IDbContextFactory<LotteryDbContext> _contextFactory;

    public LotteryConfigurationRepository(IDbContextFactory<LotteryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<LotteryConfigurationDomain?> GetActiveConfigurationAsync(int lotteryId)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.LotteryConfigurations
                .Where(c => c.LotteryId == lotteryId && (c.EndDate == null || c.EndDate > DateTime.Now))
                .OrderByDescending(c => c.StartDate) // Most recent start first
                .ThenBy(c => c.EndDate)              // Ending soonest after start
                .Select(lc => new LotteryConfigurationDomain
                {
                    Id = lc.Id,
                    LotteryId = lc.LotteryId,
                    DrawType = lc.DrawType,
                    MainNumbersCount = lc.MainNumbersCount,
                    MainNumbersRange = lc.MainNumbersRange,
                    BonusNumbersCount = lc.BonusNumbersCount.GetValueOrDefault(),
                    BonusNumbersRange = lc.BonusNumbersRange.GetValueOrDefault(),
                    StartDate = lc.StartDate,
                    EndDate = lc.EndDate,
                    CreatedAt = lc.CreatedAt,
                    UpdatedAt = lc.UpdatedAt,
                    DrawFrequency = lc.DrawFrequency,
                    IntervalDays = lc.IntervalDays,
                    DrawDays = lc.DrawDays
                })
                .FirstOrDefaultAsync();
        }
    }
}