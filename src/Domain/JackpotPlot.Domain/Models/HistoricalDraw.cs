namespace JackpotPlot.Domain.Models;

public record HistoricalDraw(int DrawId, int LotteryId, DateTime DrawDate, List<int> WinningNumbers, List<int> BonusNumbers, DateTime CreatedAt);