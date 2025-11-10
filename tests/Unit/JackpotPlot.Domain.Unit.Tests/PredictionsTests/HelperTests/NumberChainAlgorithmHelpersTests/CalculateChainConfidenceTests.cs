using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.NumberChainAlgorithmHelpersTests;

[TestFixture]
public class CalculateChainConfidenceTests
{
    private static HistoricalDraw Draw(params int[] main) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow);

    [Test]
    public void Given_Empty_Predicted_Or_No_Chains_When_CalculateChainConfidence_Method_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var draws = new[] { Draw(1, 2, 3) };
        var predicted = new List<int>();

        // Act
        var confidence = NumberChainAlgorithmHelpers.CalculateChainConfidence(draws, predicted, new Dictionary<HashSet<int>, int>(HashSet<int>.CreateSetComparer()));

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Matching_Chains_And_Numbers_When_CalculateChainConfidence_Method_Is_Invoked_Should_Return_Expected_Average()
    {
        // Arrange
        var chains = new Dictionary<HashSet<int>, int>(HashSet<int>.CreateSetComparer())
        {
            [[1, 2]] = 10,
            [[3, 4, 5]] = 9
        };
        var draws = new[] { Draw(1, 2, 3), Draw(3, 4, 5) };
        var predicted = new List<int> { 1, 2, 3 };

        // Act
        var confidence = NumberChainAlgorithmHelpers.CalculateChainConfidence(draws, predicted, chains);

        // Assert
        confidence.Should().BeApproximately(7.0 / 12.0, 1e-9);
    }
}