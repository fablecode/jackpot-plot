using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SkewnessAnalysisAlgorithmHelpersTests;

[TestFixture]
public class CalculateSkewnessTests
{
    private static HistoricalDraw Draw(params int[] numbers) =>
            new(
                DrawId: 1,
                LotteryId: 1,
                DrawDate: DateTime.UtcNow,
                WinningNumbers: numbers.ToList(),
                BonusNumbers: new List<int>(),
                CreatedAt: DateTime.UtcNow);

    [Test]
    public void Given_Fewer_Than_Two_Values_When_CalculateSkewness_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(5) };

        // Act
        var skew = SkewnessAnalysisAlgorithmHelpers.CalculateSkewness(draws);

        // Assert
        skew.Should().Be(0d);
    }

    [Test]
    public void Given_Symmetric_Data_Around_Mean_When_CalculateSkewness_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 3), Draw(1, 3) };

        // Act
        var skew = SkewnessAnalysisAlgorithmHelpers.CalculateSkewness(draws);

        // Assert
        skew.Should().Be(0d);
    }

    [Test]
    public void Given_Positive_Tail_Data_When_CalculateSkewness_Is_Invoked_Should_Return_Positive_Value()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 1, 1, 1, 10) };

        // Act
        var skew = SkewnessAnalysisAlgorithmHelpers.CalculateSkewness(draws);

        // Assert
        (skew > 0d).Should().BeTrue();
    }
}