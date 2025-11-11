using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.SkewnessAnalysisAlgorithmHelpersTests;

[TestFixture]
public class CalculateSkewnessConfidenceTests
{
    [Test]
    public void Given_Empty_Draws_When_CalculateSkewnessConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw>();
        var predicted = ImmutableArray.Create(1, 2, 3);

        // Act
        var score = SkewnessAnalysisAlgorithmHelpers.CalculateSkewnessConfidence(draws, predicted);

        // Assert
        score.Should().Be(0d);
    }

    [Test]
    public void Given_Empty_Predicted_When_CalculateSkewnessConfidence_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var predicted = ImmutableArray<int>.Empty;

        // Act
        var score = SkewnessAnalysisAlgorithmHelpers.CalculateSkewnessConfidence(draws, predicted);

        // Assert
        score.Should().Be(0d);
    }

    [Test]
    public void Given_Matching_Mean_And_Zero_Skew_When_CalculateSkewnessConfidence_Is_Invoked_Should_Return_One()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 3), Draw(1, 3) };
        var predicted = ImmutableArray.Create(2, 2);

        // Act
        var score = SkewnessAnalysisAlgorithmHelpers.CalculateSkewnessConfidence(draws, predicted);

        // Assert
        score.Should().Be(1d);
    }

    private static HistoricalDraw Draw(params int[] numbers) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: numbers.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow);

}