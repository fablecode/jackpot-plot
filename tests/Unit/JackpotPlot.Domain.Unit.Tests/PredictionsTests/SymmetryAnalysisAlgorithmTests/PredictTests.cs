using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.SymmetryAnalysisAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_PredictedNumbers()
    {
        // Arrange
        var config = CreateConfig();
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(1);

        // Act
        var result = sut.Predict(config, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var config = CreateConfig();
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(1);

        // Act
        var result = sut.Predict(config, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Zero_Confidence()
    {
        // Arrange
        var config = CreateConfig();
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(1);

        // Act
        var result = sut.Predict(config, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().Be(0d);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Set_LotteryId()
    {
        // Arrange
        var config = CreateConfig(lotteryId: 999);
        var history = new List<HistoricalDraw> { Draw(999, 2, 4, 6, 8) };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(1);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.LotteryId.Should().Be(999);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Set_AlgorithmKey()
    {
        // Arrange
        var config = CreateConfig();
        var history = new List<HistoricalDraw> { Draw(config.LotteryId, 1, 3, 5) };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(1);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.SymmetryAnalysis);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_PredictedNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40);
        var history = new List<HistoricalDraw> { Draw(config.LotteryId, 1, 2, 3, 4, 5) };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(10);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(5);
    }

    [Test]
    public void Given_ConfigRange_When_Predict_Is_Invoked_PredictedNumbers_Should_Stay_Within_Range()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 20);
        var history = new List<HistoricalDraw> { Draw(config.LotteryId, 10, 11, 12) };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(20);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= 20);
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_BonusNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 4, bonusCount: 3, bonusRange: 10);
        var history = new List<HistoricalDraw> { Draw(config.LotteryId, 1, 2, 3, 4) };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(30);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(3);
    }

    [Test]
    public void Given_No_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, bonusCount: 0, bonusRange: 0);
        var history = new List<HistoricalDraw> { Draw(config.LotteryId, 3, 6, 9) };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(40);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Keep_Bonus_In_Range()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, bonusCount: 2, bonusRange: 7);
        var history = new List<HistoricalDraw> { Draw(config.LotteryId, 3, 4, 5) };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(50);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= 7);
    }

    [Test]
    public void Given_Predictions_When_Predict_Is_Invoked_Should_Not_Overlap_PredictedNumbers_And_Bonus()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, bonusCount: 3, bonusRange: 10);
        var history = new List<HistoricalDraw> { Draw(config.LotteryId, 1, 2, 3, 4, 5) };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(60);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Intersect(result.BonusNumbers).Should().BeEmpty();
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_Non_Negative_Confidence()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, 1,3,5,7),
                Draw(config.LotteryId, 2,4,6,8)
            };
        var sut = new SymmetryAnalysisAlgorithm();
        var rng = new Random(70);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeGreaterThanOrEqualTo(0d);
    }

    private static LotteryConfigurationDomain CreateConfig(
        int mainCount = 6,
        int mainRange = 40,
        int bonusCount = 2,
        int bonusRange = 10,
        int lotteryId = 555)
        => new()
        {
            LotteryId = lotteryId,
            MainNumbersCount = mainCount,
            MainNumbersRange = mainRange,
            BonusNumbersCount = bonusCount,
            BonusNumbersRange = bonusRange
        };

    private static HistoricalDraw Draw(int lotteryId, params int[] nums) =>
        new(
            DrawId: 1,
            LotteryId: lotteryId,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: nums.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow
        );

}