using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.SkewnessAnalysisAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_And_Zero_Confidence()
    {
        // Arrange
        var config = CreateConfig();
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(42);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty && result.ConfidenceScore == 0d).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Set_LotteryId()
    {
        // Arrange
        var config = CreateConfig(lotteryId: 999);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(42);
        var sut = new SkewnessAnalysisAlgorithm();

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
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(42);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.SkewnessAnalysis);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_PredictedNumbers_Count()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 40, bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5, 6) };
        var rng = new Random(7);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(6);
    }

    [Test]
    public void Given_Config_Range_When_Predict_Is_Invoked_Should_Keep_PredictedNumbers_In_Range()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 30, bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 10, 20, 25, 30) };
        var rng = new Random(13);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= 30);
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Bonus_Count()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(2, 4, 6, 8, 10) };
        var rng = new Random(21);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(3);
    }

    [Test]
    public void Given_Bonus_Range_When_Predict_Is_Invoked_Should_Keep_Bonus_In_Range()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 2, bonusRange: 7);
        var history = new List<HistoricalDraw> { Draw(3, 6, 9, 12, 15) };
        var rng = new Random(22);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= 7);
    }

    [Test]
    public void Given_No_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 0, bonusRange: 0);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(42);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Results_When_Predict_Is_Invoked_Should_Not_Overlap_Bonus_With_PredictNumbers()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 30, bonusCount: 2, bonusRange: 15);
        var history = new List<HistoricalDraw> { Draw(1, 5, 10, 12, 20) };
        var rng = new Random(55);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Intersect(result.BonusNumbers).Should().BeEmpty();
    }

    [Test]
    public void Given_Non_Empty_History_When_Predict_Is_Invoked_Should_Return_Non_Negative_Confidence()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 20), Draw(10, 12, 14, 16, 18) };
        var rng = new Random(99);
        var sut = new SkewnessAnalysisAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        (result.ConfidenceScore >= 0d).Should().BeTrue();
    }

    private static LotteryConfigurationDomain CreateConfig(
        int mainCount = 5,
        int mainRange = 50,
        int bonusCount = 2,
        int bonusRange = 10,
        int lotteryId = 321)
        => new()
        {
            LotteryId = lotteryId,
            MainNumbersCount = mainCount,
            MainNumbersRange = mainRange,
            BonusNumbersCount = bonusCount,
            BonusNumbersRange = bonusRange
        };

    private static HistoricalDraw Draw(params int[] main) =>
        new(
            DrawId: 1,
            LotteryId: 321,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow
        );
}