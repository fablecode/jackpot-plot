using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Domain.ValueObjects;
using JackpotPlot.Primitives.Algorithms;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.MixedAlgorithmTests;

[TestFixture]
public class PredictTests
{
    // ---------- helpers ----------
    private static LotteryConfigurationDomain Config(
        int lotteryId = 7,
        int mainRange = 50, int mainCount = 5,
        int bonusRange = 10, int bonusCount = 0)
    => new()
    {
        LotteryId = lotteryId,
        MainNumbersRange = mainRange,
        MainNumbersCount = mainCount,
        BonusNumbersRange = bonusRange,
        BonusNumbersCount = bonusCount
    };

    private static PredictionResult Pr(IEnumerable<int> main, IEnumerable<int>? bonus = null, double confidence = 0.0)
    => new(
        LotteryId: new Random(100).Next(),
        PredictedNumbers: main.ToImmutableArray(),
        BonusNumbers: (bonus ?? Enumerable.Empty<int>()).ToImmutableArray(),
        ConfidenceScore: confidence,
        AlgorithmKey: "stub");

    private static IPredictionAlgorithm Algo(PredictionResult result)
    {
        var a = Substitute.For<IPredictionAlgorithm>();
        a.Predict(Arg.Any<LotteryConfigurationDomain>(), Arg.Any<IReadOnlyList<HistoricalDraw>>(), Arg.Any<Random>())
         .Returns(result);
        return a;
    }

    [Test]
    public void Given_No_Components_When_Predict_Method_Is_Invoked_Should_Return_Empty_PredictedNumbers_And_Bonus()
    {
        // Arrange
        var sut = new MixedAlgorithm(components: Array.Empty<(IPredictionAlgorithm, double)>());
        var cfg = Config(mainCount: 3, bonusCount: 2);
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty).Should().BeTrue();
    }

    [Test]
    public void Given_Components_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_Mixed()
    {
        // Arrange
        var sut = new MixedAlgorithm([
            (Algo(Pr([1])), 1.0)
        ]);
        var cfg = Config();
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.Mixed);
    }

    [Test]
    public void Given_Components_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lid = 9;
        var sut = new MixedAlgorithm([(Algo(Pr([1])), 1.0)]);
        var cfg = Config(lotteryId: lid);
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.LotteryId.Should().Be(lid);
    }

    [Test]
    public void Given_Weighted_Components_When_Predict_Method_Is_Invoked_Should_Select_PredictedNumbers_By_Weighted_Vote()
    {
        // Arrange
        var sut = new MixedAlgorithm([
            (Algo(Pr([1,2])), 1.0),
                (Algo(Pr([2,3])), 3.0)
        ]);
        var cfg = Config(mainCount: 1);
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Single().Should().Be(2);
    }

    [Test]
    public void Given_Negative_Weight_Component_When_Predict_Method_Is_Invoked_Should_Treat_Negative_As_Zero()
    {
        // Arrange
        var sut = new MixedAlgorithm([
            (Algo(Pr([9])), -10.0),
            (Algo(Pr([7])),   1.0)
        ]);
        var cfg = Config(mainCount: 1);
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Single().Should().Be(7);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Select_Most_Frequent_Bonus()
    {
        // Arrange
        var sut = new MixedAlgorithm([
            (Algo(Pr([1], [9])),  1.0),
            (Algo(Pr([2], [9])),  2.0),
            (Algo(Pr([3], [8])),  3.0)
        ]);
        var cfg = Config(bonusCount: 1, bonusRange: 12);
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.BonusNumbers.Single().Should().Be(9);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new MixedAlgorithm([(Algo(Pr([1], [9])), 1.0)]);
        var cfg = Config(bonusCount: 0);
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Weighted_Components_When_Predict_Method_Is_Invoked_Should_Return_Weighted_Average_Confidence()
    {
        // Arrange
        var sut = new MixedAlgorithm([
            (Algo(Pr([1], confidence: 0.2)), 1.0),
            (Algo(Pr([2], confidence: 0.8)), 3.0)
        ]);
        var cfg = Config();
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().BeApproximately(0.65, 1e-9);
    }
}