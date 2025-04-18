﻿using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Services;

public interface IEurojackpotService
{
    IEnumerable<EurojackpotResult> GetAllDrawHistoryResults();
    IAsyncEnumerable<EurojackpotResult> GetAllDrawHistoryResultsAsync();
}