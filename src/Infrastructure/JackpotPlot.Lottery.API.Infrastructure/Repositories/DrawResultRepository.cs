using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public sealed class DrawResultRepository : IDrawResultRepository
{
    private readonly IDbContextFactory<LotteryDbContext> _contextFactory;

    public DrawResultRepository(IDbContextFactory<LotteryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> Add(int drawId, EurojackpotResult draw)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var newDrawResult = new DrawResult
            {
                DrawId = drawId,
                Numbers = draw.MainNumbers.ToList(),
                BonusNumbers = draw.EuroNumbers.ToList()
            };

            var addedDrawResult = await context.DrawResults.AddAsync(newDrawResult);

            return addedDrawResult.Entity.Id;
        }
    }
}