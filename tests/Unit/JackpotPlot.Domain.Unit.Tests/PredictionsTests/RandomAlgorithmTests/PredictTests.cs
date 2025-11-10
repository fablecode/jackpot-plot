using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.RandomAlgorithmTests;

[TestFixture]
public class PredictTests
{
    // ---------- helpers ----------
    private static LotteryConfigurationDomain Config(
        int lotteryId = 7,
        int mainRange = 50,
        int mainCount = 6,
        int bonusRange = 12,
        int bonusCount = 0)
    {
        return new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersRange = mainRange,
            MainNumbersCount = mainCount,
            BonusNumbersRange = bonusRange,
            BonusNumbersCount = bonusCount
        };
    }

    [Test]
    public void Given_Config_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_Random()
    {
        // Arrange
        var sut = new RandomAlgorithm();
        var cfg = Config();
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.Random);
    }

    [Test]
    public void Given_Config_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lid = 9;
        var sut = new RandomAlgorithm();
        var cfg = Config(lotteryId: lid);
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.LotteryId.Should().Be(lid);
    }

    [Test]
    public void Given_Config_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new RandomAlgorithm();
        var cfg = Config(mainCount: 5);
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_Config_When_Predict_Method_Is_Invoked_Should_Return_PredictedNumbers_In_Range()
    {
        // Arrange
        var sut = new RandomAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 6);
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_Config_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictedNumbers()
    {
        // Arrange
        var sut = new RandomAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6);
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new RandomAlgorithm();
        var cfg = Config(bonusCount: 0);
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new RandomAlgorithm();
        var cfg = Config(bonusRange: 15, bonusCount: 3);
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range_And_Distinct()
    {
        // Arrange
        var sut = new RandomAlgorithm();
        var cfg = Config(bonusRange: 10, bonusCount: 4);
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         result.BonusNumbers.Distinct().Count() == result.BonusNumbers.Length).Should().BeTrue();
    }

    [Test]
    public void Given_Config_When_Predict_Is_Called_Should_Return_Expected_Confidence()
    {
        // Arrange
        var sut = new RandomAlgorithm();
        var cfg = Config(mainRange: 50, mainCount: 6);
        var rng = new Random(9);
        var expected = 1.0 / (cfg.MainNumbersRange - cfg.MainNumbersCount + 1);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().BeApproximately(expected, 1e-12);
    }
}