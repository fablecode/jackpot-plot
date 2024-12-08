using JackpotPlot.Domain.Repositories;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public sealed class LotteryRepository : ILotteryRepository
{
    private readonly IDbContextFactory<LotteryDbContext> _contextFactory;

    public LotteryRepository(IDbContextFactory<LotteryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    public async Task<int> GetLotteryIdByName(string name)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            return await context.Lotteries
                .Where(l => EF.Functions.ILike(l.Name, name))
                .Select(l => l.Id)
                .SingleAsync();
        }
    }
}