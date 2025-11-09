using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.NumberSumAlgorithmHelpersTests;

[TestFixture]
public class CalculateTargetSumTests
{
    [Test]
    public void Given_Draws_When_CalculateTargetSum_Method_Is_Invoked_Should_Return_Average_Of_Sums()
    {
        // Arrange
        var draws = new List<HistoricalDraw>
            {
                Draw(1, 1,2,3),   // sum 6
                Draw(2, 4,5)      // sum 9
            };

        // Act
        var target = NumberSumAlgorithmHelpers.CalculateTargetSum(draws);

        // Assert
        target.Should().Be(7.5);
    }

    private static HistoricalDraw Draw(int id, params int[] main) =>
        new(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow.AddDays(id));

}