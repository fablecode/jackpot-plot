using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.QuadrantAnalysisAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_QuadrantAnalysis()
    {
        // Arrange
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 1, 10, 20) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.QuadrantAnalysis);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lid = 14;
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(lotteryId: lid);
        var history = new List<HistoricalDraw> { Draw(1, 2, 15, 30) };
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lid);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_Empty_PredictedNumbers_And_Bonus_And_ConfidenceScore_Zero()
    {
        // Arrange
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(mainCount: 5, bonusCount: 2);
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty && result.ConfidenceScore == 0.0).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 1, 10, 20, 30) };
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_PredictedNumbers_In_Range()
    {
        // Arrange
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(mainRange: 24, mainCount: 5);
        var history = new List<HistoricalDraw> { Draw(1, 2, 6, 12, 18, 24) };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictedNumbers()
    {
        // Arrange
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 1, 11, 21, 31) };
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 5, 10, 15, 20) };
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
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(1, 5, 10, 15, 20) };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range_And_Not_Overlap_Main()
    {
        // Arrange
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw> { Draw(1, 5, 10, 15, 20) };
        var rng = new Random(9);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         !result.BonusNumbers.Intersect(result.PredictedNumbers).Any()).Should().BeTrue();
    }

    [Test]
    public void Given_Equal_Quadrant_Distribution_When_Predict_Method_Is_Invoked_Should_Return_ConfidenceScore_Computed_By_Helper()
    {
        // Arrange
        var sut = new QuadrantAnalysisAlgorithm();
        var cfg = Config(mainRange: 8, mainCount: 4, bonusCount: 0);
        var history = new List<HistoricalDraw>
        {
            Draw(1, 1, 3, 5, 7),
            Draw(2, 2, 4, 6, 8),
            Draw(3, 1, 3, 5, 7),
            Draw(4, 2, 4, 6, 8)
        };
        var rng = new Random(10);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        var quads = QuadrantAnalysisAlgorithmHelpers.DivideIntoQuadrants(cfg.MainNumbersRange, 4);
        var expected = QuadrantAnalysisAlgorithmHelpers.CalculateQuadrantConfidence(history, result.PredictedNumbers.ToList(), quads);
        result.ConfidenceScore.Should().Be(expected);
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
        int lotteryId = 2,
        int mainRange = 40,
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
}