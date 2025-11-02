using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HighLowNumberSplitAlgorithmTests;

[TestFixture]
public class PredictTests
{
    // ---------- helpers ----------
    private static HistoricalDraw Draw(params int[] main) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow);

    private static LotteryConfigurationDomain Config(
        int lotteryId = 13,
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
    public void Given_No_History_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_Zero()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(mainCount: 6);
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().Be(0.0);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_PredictionNumbers_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(mainCount: 5);
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_HighLowNumberSplit()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 2, 40) };
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.HighLowNumberSplit);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lotteryId = 23;
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(lotteryId: lotteryId);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_PredictionNumbers_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 5, 7, 30) };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_PredictionNumbers_In_Range()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 5);
        var history = new List<HistoricalDraw> { Draw(1, 2, 19) };
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictionNumbers()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(mainRange: 30, mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 2, 15, 25) };
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    [Test]
    public void Given_LowRatio_Rounds_Away_From_Zero_When_Predict_Method_Is_Invoked_Should_Apply_Rounded_Low_Count()
    {
        // Arrange
        // mainCount=5, suppose lowRatio≈0.51 -> lowCount=Round(2.55)=3 (AwayFromZero)
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(mainRange: 50, mainCount: 5);
        // Make lows slightly dominate: more values <= 25 than >25
        var history = new List<HistoricalDraw>
            {
                Draw(3, 6, 12, 26),   // 3 lows, 1 high
                Draw(7, 14, 20, 40),  // 3 lows, 1 high
                Draw(2, 27, 28)       // 1 low, 2 highs
            };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        (result.PredictedNumbers.Count(n => n <= cfg.MainNumbersRange / 2) >= 3).Should().BeTrue();
    }

    // ---------- bonus ----------
    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 2, 30) };
        var rng = new Random(9);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_Count_Equals_Config()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(1, 2, 40) };
        var rng = new Random(10);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range_And_Not_Overlap_PredictionNumbers()
    {
        // Arrange
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw> { Draw(5, 10, 20, 30) };
        var rng = new Random(11);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         !result.BonusNumbers.Intersect(result.PredictedNumbers).Any()).Should().BeTrue();
    }

    [Test]
    public void Given_Predicted_Distribution_Matches_History_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_One()
    {
        // Arrange
        // History split 50/50; prediction will also be split ~50/50 from same halves.
        var sut = new HighLowNumberSplitAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 4, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 2, 15, 16), // 2 low, 2 high
                Draw(3, 4, 17, 18)  // 2 low, 2 high
            };
        var rng = new Random(12);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(1.0);
    }
}