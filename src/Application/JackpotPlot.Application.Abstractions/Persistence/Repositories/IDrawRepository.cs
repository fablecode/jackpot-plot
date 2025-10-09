using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Application.Abstractions.Persistence.Repositories;

public interface IDrawRepository
{
    Task<int> Add(int lotteryId, EurojackpotResult draw);
    Task<bool> DrawExist(int lotteryId, DateTime drawDate, ImmutableArray<int> mainNumbers,
        ImmutableArray<int> bonusNumbers);
}