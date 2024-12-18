using System.Collections.Immutable;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Repositories;

public interface ILotteryHistoryRepository
{
    Task<int> Add(LotteryDrawnEvent lotteryDrawnEvent);
    Task<ICollection<HistoricalDraw>> GetHistoricalDraws(int lotteryId);
}