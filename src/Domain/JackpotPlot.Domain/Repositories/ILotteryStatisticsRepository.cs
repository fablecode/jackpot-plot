using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;

namespace JackpotPlot.Domain.Repositories;

public interface ILotteryStatisticsRepository
{
    Task<ImmutableArray<NumberStatus>> GetHotColdNumbers(int lotteryId, List<int> numbers, TimeSpan timeRange, string numberType);
}