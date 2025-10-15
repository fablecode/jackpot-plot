using JackpotPlot.Domain.Models;
using System.Runtime.CompilerServices;

namespace JackpotPlot.Application.Abstractions.Services;

public interface IEurojackpotService
{
    IEnumerable<EurojackpotResult> GetAllDrawHistoryResults();
    IAsyncEnumerable<EurojackpotResult> GetAllDrawHistoryResultsAsync(CancellationToken cancellationToken = default);
}