using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Domain.ValueObjects;
using NUnit.Framework;
using System.Collections.Immutable;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.MixedAlgorithmHelpersTests;

[TestFixture]
public class CombineBonusNumbersTests
{
    // --- tiny factory for readable test inputs ---
    private static PredictionResult Pr(
        IEnumerable<int> main,
        IEnumerable<int>? bonus = null,
        double confidence = 0.0)
    {
        // Adjust the ctor/initializer if your PredictionResult differs.
        return new PredictionResult(
            LotteryId: new Random(100).Next(),
            AlgorithmKey: "x",
            PredictedNumbers: main.ToImmutableArray(),
            BonusNumbers: (bonus ?? []).ToImmutableArray(),
            ConfidenceScore: confidence
        );
    }

    [Test]
    public void Given_Non_Positive_Take_When_CombineBonusNumbers_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var inputs = new List<(PredictionResult, double)>
        {
            (Pr([1], [9]), 1.0)
        };

        // Act
        var result = MixedAlgorithmHelpers.CombineBonusNumbers(inputs, take: 0).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Multiple_Results_When_CombineBonusNumbers_Method_Is_Invoked_Should_Return_Most_Frequent_Bonus_First()
    {
        // Arrange
        // bonus frequencies: 9→3, 8→1, 7→1 → top is 9
        var inputs = new List<(PredictionResult, double)>
        {
            (Pr([1], [9,8]), 1.0),
            (Pr([2], [9]),   2.0),
            (Pr([3], [9,7]), 1.5),
        };

        // Act
        var top = MixedAlgorithmHelpers.CombineBonusNumbers(inputs, take: 1).Single();

        // Assert
        top.Should().Be(9);
    }
}