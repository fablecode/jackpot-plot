using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests;

public static class AlgorithmsTestHelperTests
{
    public static HistoricalDraw Draw(params int[] main) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow);
}