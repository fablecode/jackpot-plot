using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.DrawPositionAnalysisAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_Empty_Main_And_Bonus()
    {
        // Arrange
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(mainCount: 5, bonusCount: 2);
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty).Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_Confidence_Zero()
    {
        // Arrange
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(mainCount: 3);
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().Be(0.0);
    }

    // ---------- metadata ----------
    [Test]
    public void Given_Valid_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_DrawPositionAnalysis()
    {
        // Arrange
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.DrawPositionAnalysis);
    }

    [Test]
    public void Given_Valid_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lotteryId = 6;
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(lotteryId: lotteryId);
        var history = new List<HistoricalDraw> { Draw(7, 8, 9) };
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    // ---------- main numbers behavior ----------
    [Test]
    public void Given_Valid_History_When_Predict_Method_Is_Invoked_PredictionNumbers_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 4);
        var history = new List<HistoricalDraw>
            {
                Draw(1,2,3,4),
                Draw(5,6,7,8)
            };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_Valid_History_When_Predict_Method_Is_Invoked_PredictionNumbers_Should_Be_In_Range()
    {
        // Arrange
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 3);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3), Draw(4, 5, 6) };
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_Repeated_Top_By_Position_When_Predict_Method_Is_Invoked_Should_Return_Distinct_MainNumbers()
    {
        // Arrange
        // Position 0 heavily favors 7; position 1 also favors 7, helper avoids duplicates across positions.
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 2);
        var history = new List<HistoricalDraw>
            {
                Draw(7,7),
                Draw(7,7),
                Draw(7,7)
            };
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    // ---------- bonus generation ----------
    [Test]
    public void Given_Bonus_Count_Zero_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Positive_BonusCount_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3), Draw(4, 5, 6) };
        var rng = new Random(9);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_Bonus_Generation_When_Predict_Method_Is_Invoked_Bonus_In_Range_And_Not_Overlap_PredictionNumbers()
    {
        // Arrange
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(mainRange: 30, mainCount: 3, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw> { Draw(5, 6, 7), Draw(1, 2, 3) };
        var rng = new Random(10);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         !result.BonusNumbers.Intersect(result.PredictedNumbers).Any()).Should().BeTrue();
    }

    [Test]
    public void Given_Single_Position_Always_Matching_When_Predict_Method_Is_Invoked_Should_Return_Confidence_One()
    {
        // Arrange
        // History: position 0 always 42. With mainCount=1, top pick for pos0 is 42; every draw matches.
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(mainRange: 60, mainCount: 1, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(42, 1, 2),
                Draw(42, 5, 6),
                Draw(42, 7, 8),
            };
        var rng = new Random(11);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(1.0);
    }

    [Test]
    public void Given_No_Position_Matches_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_ZeroPoint_Five()
    {
        // Arrange
        // History alternates numbers so predicted top-per-position won't match positions for given mainCount.
        var sut = new DrawPositionAnalysisAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 2, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 2),
                Draw(2, 1),
                Draw(1, 2),
                Draw(2, 1),
            };
        var rng = new Random(12);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeApproximately(0.5, 1e-9);
    }

    // ---------- helpers ----------
    private static HistoricalDraw Draw(params int[] main) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(), // position = index as drawn
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow);

    private static LotteryConfigurationDomain Config(
        int lotteryId = 9,
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