using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.NumberChainAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeNumberChainsTests
{
    private static HistoricalDraw Draw(params int[] main) =>
        new
        (
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: [],
            CreatedAt: DateTime.UtcNow
        );

    [Test]
    public void Given_Single_Draw_When_AnalyzeNumberChains_Method_Is_Invoked_Should_Produce_All_Pairs_And_Triplets()
    {
        // Arrange
        var draws = new[] { Draw(1, 2, 3) };

        // Act
        var result = NumberChainAlgorithmHelpers.AnalyzeNumberChains(draws);

        // Assert
        result.Count.Should().Be(4);
    }

    [Test]
    public void Given_Multiple_Draws_When_AnalyzeNumberChains_Method_Is_Invoked_Should_Order_By_Frequency_Descending()
    {
        // Arrange
        var draws = new[] { Draw(1, 2, 3), Draw(1, 2, 4) };

        // Act
        var result = NumberChainAlgorithmHelpers.AnalyzeNumberChains(draws);

        // Assert
        result.First().Value.Should().Be(2);
    }
}