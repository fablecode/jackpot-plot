using System.Collections.Immutable;

namespace JackpotPlot.Domain.Models;

public sealed class LotteryDrawnEvent
{
    public int LotteryId { get; set; }
    public DateTime DrawDate { get; set; }
    public ImmutableArray<int> WinningNumbers { get; set; }
    public ImmutableArray<int> BonusNumbers { get; set; }
}