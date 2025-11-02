using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.HighLowNumberSplitAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeHighLowSplitTests
{
    [Test]
    public void Given_No_Draws_When_AnalyzeHighLowSplit_Method_Is_Invoked_Should_Return_Equal_Ratios()
    {
        // Arrange
        var draws = Array.Empty<HistoricalDraw>();

        // Act
        var (low, high) = HighLowNumberSplitAlgorithmHelpers.AnalyzeHighLowSplit(draws, numberRange: 10);

        // Assert
        (low == 0.5 && high == 0.5).Should().BeTrue();
    }

    [Test]
    public void Given_Mixed_Draws_When_AnalyzeHighLowSplit_Method_Is_Invoked_Should_Return_Correct_Ratios()
    {
        // Arrange
        // numberRange=10 ⇒ mid=5 → low: ≤5, high: >5
        // lows: [1,2] + [5] = 3; highs: [9] + [6] = 2 → lowRatio 3/5 = 0.6, highRatio 0.4
        var draws = new[]
        {
            AlgorithmsTestHelperTests.Draw(1, 2, 9),
            AlgorithmsTestHelperTests.Draw(6, 5)
        };

        // Act
        var (low, high) = HighLowNumberSplitAlgorithmHelpers.AnalyzeHighLowSplit(draws, numberRange: 10);

        // Assert
        low.Should().BeApproximately(0.6, 1e-9);
    }
}