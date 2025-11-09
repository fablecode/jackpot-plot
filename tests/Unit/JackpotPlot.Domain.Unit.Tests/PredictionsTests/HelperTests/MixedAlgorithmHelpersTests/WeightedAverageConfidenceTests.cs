using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Domain.ValueObjects;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.MixedAlgorithmHelpersTests;

[TestFixture]
public class WeightedAverageConfidenceTests
{
    [Test]
    public void Given_Zero_Total_Weight_When_WeightedAverageConfidence_Is_Called_Should_Return_Zero()
    {
        // Arrange
        var inputs = new List<(PredictionResult, double)>
        {
            (Pr([1], confidence: 1.0),  1.0),
            (Pr([2], confidence: 0.0), -1.0), // total weight = 0
        };

        // Act
        var conf = MixedAlgorithmHelpers.WeightedAverageConfidence(inputs);

        // Assert
        conf.Should().Be(0.0);
    }

    [Test]
    public void Given_Positive_Weights_When_WeightedAverageConfidence_Is_Called_Should_Return_Weighted_Mean()
    {
        // Arrange
        var inputs = new List<(PredictionResult, double)>
        {
            (Pr([1], confidence: 0.2), 1.0),
            (Pr([2], confidence: 0.8), 3.0),
        };

        // Act
        var conf = MixedAlgorithmHelpers.WeightedAverageConfidence(inputs);

        // Assert
        conf.Should().BeApproximately(0.65, 1e-9);
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