using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.InvertedFrequencyAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_InvertedFrequency()
    {
        // Arrange
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.InvertedFrequency);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lotteryId = 89;
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(lotteryId: lotteryId);
        var history = new List<HistoricalDraw> { Draw(1, 4, 5, 6) };
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_Confidence_Zero()
    {
        // Arrange
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(mainCount: 5);
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().Be(0.0);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_PredictionNumbers_Count_Equals_Config()
    {
        // Arrange
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6);
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_Hot_And_Cold_Numbers_When_Predict_Method_Is_Invoked_Should_Pick_From_Coldest_Set()
    {
        // Arrange
        // Make 1..5 “hot” by appearing; 6..10 “cold” (never seen). Expect all picks from 6..10.
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 3);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1,2,3,4,5),
                Draw(2, 1,2,3,4,5),
                Draw(3, 1,2,3,4,5),
            };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 6 && n <= 10).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_PredictedNumbers_In_Range()
    {
        // Arrange
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(mainRange: 18, mainCount: 5);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictedNumbers()
    {
        // Arrange
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(mainRange: 25, mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4) };
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4) };
        var rng = new Random(9);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range_And_Not_Overlap_PredictedNumbers()
    {
        // Arrange
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 5, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(10);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         !result.BonusNumbers.Intersect(result.PredictedNumbers).Any()).Should().BeTrue();
    }

    [Test]
    public void Given_Common_Overlap_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_Greater_Than_Zero()
    {
        // Arrange
        // numberRange=8 → unseen coldest group = {8} (size 1) < mainCount (4)
        // So the algorithm must pick 1 from {8} and 3 from the freq=1 group {1..7} → guaranteed overlap > 0.
        var sut = new InvertedFrequencyAlgorithm();
        var cfg = Config(mainRange: 8, mainCount: 4, bonusCount: 0);
        var history = new List<HistoricalDraw>
        {
            Draw(1, 1, 2, 3),
            Draw(2, 4, 5, 6),
            Draw(3, 7),
        };
        var rng = new Random(11);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeGreaterThan(0.0);
    }

    // ---------- helpers ----------
    private static HistoricalDraw Draw(int id, params int[] main) =>
        new(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: [],
            CreatedAt: DateTime.UtcNow.AddDays(id));

    private static LotteryConfigurationDomain Config(
        int lotteryId = 6,
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