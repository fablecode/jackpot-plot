using System.Collections.Immutable;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;

namespace JackpotPlot.Lottery.API.Infrastructure.Repositories;

public class DrawRepository : IDrawRepository
{
    public Task<int> Add(int lotteryId, EurojackpotResult draw)
    {
        throw new NotImplementedException();
    }

    public bool DrawExist(int lotteryId, DateTime drawDate, ImmutableArray<int> mainNumbers, ImmutableArray<int> bonusNumbers)
    {
        throw new NotImplementedException();
    }
}