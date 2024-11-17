using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Services;

public interface IEurojackpotService
{
    IEnumerable<EurojackpotResult> GetAllDrawHistoryResults();
}