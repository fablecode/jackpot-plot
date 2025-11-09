using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.LastAppearanceAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_LastAppearance()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 1, 3) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.LastAppearance);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lotteryId = 8;
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(lotteryId: lotteryId);
        var history = new List<HistoricalDraw> { Draw(1, 2, 4) };
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Select_Lowest_N_As_Overdue()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 4);
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, new List<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Should().Equal(1, 2, 3, 4);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_Zero()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(mainCount: 5);
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, new List<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().Be(0.0);
    }

    [Test]
    public void Given_Unseen_Numbers_When_Predict_Method_Is_Invoked_Should_Prioritize_Unseen_Then_Longest_Since()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(mainRange: 6, mainCount: 3);
        var history = new List<HistoricalDraw> { Draw(1, 3, 4) };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Should().BeEquivalentTo([1, 2, 5]);
    }

    [Test]
    public void Given_Range_Equals_History_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_Greater_Than_Zero()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(mainRange: 3, mainCount: 2, bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 1), Draw(2, 2), Draw(3, 3) };
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeGreaterThan(0.0);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 5, 7, 30) };
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_PredictedNumbers_In_Range()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 5);
        var history = new List<HistoricalDraw> { Draw(1, 2, 19) };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictedNumbers()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(mainRange: 25, mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4) };
        var rng = new Random(9);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 2, 30) };
        var rng = new Random(10);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(1, 2, 40) };
        var rng = new Random(11);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range()
    {
        // Arrange
        var sut = new LastAppearanceAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 5, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(12);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange).Should().BeTrue();
    }

    // ---------- helpers ----------
    private static HistoricalDraw Draw(int id, params int[] main) =>
        new(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow.AddDays(id));

    private static LotteryConfigurationDomain Config(
        int lotteryId = 2,
        int mainRange = 50,
        int mainCount = 5,
        int bonusRange = 10,
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
}