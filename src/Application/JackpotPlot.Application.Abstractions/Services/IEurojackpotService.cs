using JackpotPlot.Domain.Models;

namespace JackpotPlot.Application.Abstractions.Services;

public interface IEurojackpotService
{
    IEnumerable<EurojackpotResult> GetAllDrawHistoryResults();
    IAsyncEnumerable<EurojackpotResult> GetAllDrawHistoryResultsAsync();
}