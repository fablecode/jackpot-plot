using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.PatternMatchingAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_PatternMatching()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 1, 6) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.PatternMatching);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lid = 123;
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(lotteryId: lid);
        var history = new List<HistoricalDraw> { Draw(1, 1, 6) };
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lid);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_Empty_PredictedNumbers_Bonus_And_Confidence_Zero()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config();
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty && result.ConfidenceScore == 0.0).Should().BeTrue();
    }

    [Test]
    public void Given_History_With_Patterns_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 4);
        var history = new List<HistoricalDraw> { Draw(1, 1, 6, 3, 8) }; // has correct count
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_With_Patterns_When_Predict_Method_Is_Invoked_Should_Return_PredictedNumbers_In_Range()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 3);
        var history = new List<HistoricalDraw> { Draw(1, 2, 9, 15) };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_All_Draws_With_Wrong_Count_When_Predict_Method_Is_Invoked_Should_Fallback_To_Random_And_Return_Count()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 3);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1, 6),     // count 2
                Draw(2, 3, 4, 5),  // count 3 -> ok? we need all wrong -> change to 4
                Draw(3, 7)         // count 2
            };
        history[1] = Draw(2, 3, 4, 5);

        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 1, 6) };
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(bonusCount: 2, bonusRange: 10);
        var history = new List<HistoricalDraw> { Draw(1, 1, 6) };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range_And_Not_Overlap_PredictedNumbers()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 4, bonusRange: 10, bonusCount: 3);
        var history = new List<HistoricalDraw> { Draw(1, 1, 6, 7, 12) };
        var rng = new Random(9);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         !result.BonusNumbers.Intersect(result.PredictedNumbers).Any()).Should().BeTrue();
    }

    [Test]
    public void Given_Dominant_Pattern_When_Predict_Method_Is_Invoked_Should_Return_Expected_ConfidenceScore()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 2, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1, 6), // OL,EH
                Draw(2, 3, 8), // OL,EH
                Draw(3, 1, 2)  // OL,EL (different)
            };
        var rng = new Random(10);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeApproximately(2.0 / 3.0, 1e-9);
    }

    [Test]
    public void Given_All_Draws_With_Wrong_Count_When_Predict_Method_Is_Invoked_Should_Set_ConfidenceScore_To_One()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 3, bonusCount: 0);
        var history = new List<HistoricalDraw>
        {
            Draw(1, 1, 6),     // count 2 (wrong)
            Draw(2, 3, 4, 5),  // count 4 (wrong)
            Draw(3, 7, 8)      // count 2 (wrong)
        };
        var rng = new Random(20);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(1.0);
    }

    [Test]
    public void Given_Dominant_Pattern_When_Predict_Method_Is_Invoked_Should_Produce_Numbers_Matching_That_Pattern()
    {
        // Arrange
        var sut = new PatternMatchingAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 2, bonusCount: 0);
        var history = new List<HistoricalDraw>
        {
            Draw(1, 1, 6), // OL,EH
            Draw(2, 3, 8), // OL,EH
            Draw(3, 2, 4)  // EL,EH (different)
        };
        var rng = new Random(21);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        var half = cfg.MainNumbersRange / 2;
        var predictedPattern = string.Join(",",
            result.PredictedNumbers.Select(n =>
                $"{(n % 2 == 1 ? "O" : "E")}{(n <= half ? "L" : "H")}"));
        predictedPattern.Should().Be("OL,EH");
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
        int lotteryId = 7,
        int mainRange = 10,
        int mainCount = 2,
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