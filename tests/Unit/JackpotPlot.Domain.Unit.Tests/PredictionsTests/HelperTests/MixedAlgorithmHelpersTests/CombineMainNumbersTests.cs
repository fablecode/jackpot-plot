using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Domain.ValueObjects;
using NUnit.Framework;
using System.Collections.Immutable;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.MixedAlgorithmHelpersTests;

[TestFixture]
public class CombineMainNumbersTests
{
    [Test]
    public void Given_Non_Positive_Take_When_CombineMainNumbers_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(1);
        var inputs = new List<(PredictionResult, double)>
            {
                (Pr([1,2]), 1.0)
            };

        // Act
        var result = MixedAlgorithmHelpers.CombineMainNumbers(inputs, take: 0, rng).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_Overlapping_Numbers_When_CombineMainNumbers_Method_Is_Invoked_Should_Pick_Number_With_Highest_Total_Weight()
    {
        // Arrange
        var rng = new Random(2);
        var inputs = new List<(PredictionResult, double)>
            {
                (Pr([3,5]), 3.0),
                (Pr([5,9]), 4.0),
                (Pr([3]),   2.0)
            };

        // Act
        var result = MixedAlgorithmHelpers.CombineMainNumbers(inputs, take: 1, rng).Single();

        // Assert
        result.Should().Be(5);
    }

    [Test]
    public void Given_Negative_Weights_When_CombineMainNumbers_Method_Is_Invoked_Should_Treat_Negatives_As_Zero()
    {
        // Arrange
        var rng = new Random(3);
        var inputs = new List<(PredictionResult, double)>
            {
                (Pr([99]), -10.0),
                (Pr([7]),   1.0),
            };

        // Act
        var result = MixedAlgorithmHelpers.CombineMainNumbers(inputs, take: 1, rng).Single();

        // Assert
        result.Should().Be(7);
    }

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
}