using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Repositories;

public interface IDrawResultRepository
{
    Task<int> Add(int drawId, EurojackpotResult draw);
}