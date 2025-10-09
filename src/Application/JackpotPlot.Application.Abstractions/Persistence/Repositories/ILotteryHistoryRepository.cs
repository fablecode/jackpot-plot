using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Application.Abstractions.Persistence.Repositories;

public interface ILotteryHistoryRepository
{
    Task<(Dictionary<int, int> hotNumbers, Dictionary<int, int> coldNumbers)> GetAll();
    Task<int> Add(LotteryDrawnEvent lotteryDrawnEvent);
    Task<ICollection<HistoricalDraw>> GetHistoricalDraws(int lotteryId);
    Task<ImmutableArray<WinningNumberFrequencyResult>> GetWinningNumberFrequency();
    Task<ImmutableArray<WinningNumberMovingAverageResult>> GetMovingAverageWinningNumbers(int lotteryId, int windowSize);
}