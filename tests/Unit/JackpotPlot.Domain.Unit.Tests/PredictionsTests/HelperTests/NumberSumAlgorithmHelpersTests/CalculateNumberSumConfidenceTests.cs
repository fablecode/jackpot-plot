using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.NumberSumAlgorithmHelpersTests;

public class CalculateNumberSumConfidenceTests
{
    [Test]
    public void Given_Empty_Predicted_When_CalculateNumberSumConfidence_Method_Is_Invoked_Confidence_Should_Be_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 1, 2, 3) };
        var predicted = ImmutableArray<int>.Empty;

        // Act
        var confidence = NumberSumAlgorithmHelpers.CalculateNumberSumConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Empty_History_When_CalculateNumberSumConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new List<HistoricalDraw>();
        var predicted = ImmutableArray.Create(1, 2, 3);

        // Act
        var confidence = NumberSumAlgorithmHelpers.CalculateNumberSumConfidence(draws, predicted);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Known_Deviations_When_CalculateNumberSumConfidence_Method_Is_Invoked_Should_Return_Expected_Value()
    {
        // Arrange
        var draws = new List<HistoricalDraw> { Draw(1, 4, 6), Draw(2, 7, 7) };
        var predicted = ImmutableArray.Create(6, 6); // sum 12

        // Act
        var confidence = NumberSumAlgorithmHelpers.CalculateNumberSumConfidence(draws, predicted);

        // Assert
        confidence.Should().BeApproximately(1.0 / 3.0, 1e-9);
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