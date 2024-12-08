using System.Collections.Immutable;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using JackpotPlot.Lottery.API.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public class DrawRepository : IDrawRepository
{
    private readonly IDbContextFactory<LotteryDbContext> _contextFactory;

    public DrawRepository(IDbContextFactory<LotteryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> Add(int lotteryId, EurojackpotResult draw)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var newDraw = new Draw
            {
                LotteryId = lotteryId,
                DrawDate = DateOnly.FromDateTime(draw.Date),
                JackpotAmount = decimal.Parse(draw.JackpotAmount),
                RolloverCount = draw.Rollover
            };

            var addedDraw = await context.Draws.AddAsync(newDraw);

            return addedDraw.Entity.Id;
        }
    }

    public async Task<bool> DrawExist(int lotteryId, DateTime drawDate, ImmutableArray<int> mainNumbers, ImmutableArray<int> bonusNumbers)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var result = LotteryQueries.CheckDrawAndResultsCompiled(context, lotteryId, drawDate, mainNumbers.ToArray(), bonusNumbers.ToArray());

            return result;
        }
    }
}