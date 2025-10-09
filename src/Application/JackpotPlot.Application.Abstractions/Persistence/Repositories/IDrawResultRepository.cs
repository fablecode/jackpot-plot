using JackpotPlot.Domain.Models;

namespace JackpotPlot.Application.Abstractions.Persistence.Repositories;

public interface IDrawResultRepository
{
    Task<int> Add(int drawId, EurojackpotResult draw);
}