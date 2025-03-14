using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Repositories;

public interface ILotteryHistoryRepository
{
    Task<(Dictionary<int, int> hotNumbers, Dictionary<int, int> coldNumbers)> GetAll();
    Task<int> Add(LotteryDrawnEvent lotteryDrawnEvent);
    Task<ICollection<HistoricalDraw>> GetHistoricalDraws(int lotteryId);
    Task<ImmutableArray<WinningNumberFrequencyResult>> GetWinningNumberFrequency();
}