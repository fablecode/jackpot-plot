using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.GroupSelectionAlgorithmTests;

[TestFixture]
public class PredictTests
{
    private static HistoricalDraw Draw(int lotteryId, params int[] main) =>
        new(
            DrawId: 1,
            LotteryId: lotteryId,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow
        );

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_PredictedNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 40, bonusCount: 0);
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(1);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(6);
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_BonusNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 3, bonusRange: 12);
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(2);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(3);
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Set_LotteryId()
    {
        // Arrange
        var config = CreateConfig(lotteryId: 999);
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(3);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.LotteryId.Should().Be(999);
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Set_AlgorithmKey()
    {
        // Arrange
        var config = CreateConfig();
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(4);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.GroupSelection);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_PredictedNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 45, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, 1, 2, 3, 4, 5, 6),
                Draw(config.LotteryId, 10, 20, 30, 40, 45, 5)
            };
        var rng = new Random(5);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(6);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Keep_PredictedNumbers_Within_Config_Range()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 30, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, 1, 15, 30),
                Draw(config.LotteryId, 5, 10, 20, 25)
            };
        var rng = new Random(6);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should()
            .OnlyContain(n => n >= 1 && n <= 30);
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_BonusNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, 1, 2, 3, 4, 5)
            };
        var rng = new Random(7);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(3);
    }

    [Test]
    public void Given_No_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 0, bonusRange: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, 1, 2, 3, 4, 5)
            };
        var rng = new Random(8);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Keep_BonusNumbers_Within_Config_Range()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 2, bonusRange: 7);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, 3, 5, 7, 9, 11)
            };
        var rng = new Random(9);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Should()
            .OnlyContain(n => n >= 1 && n <= 7);
    }

    [Test]
    public void Given_Predictions_When_Predict_Is_Invoked_Should_Not_Overlap_PredictedNumbers_And_Bonus()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 50, bonusCount: 3, bonusRange: 10);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, 1, 2, 3, 4, 5, 6),
                Draw(config.LotteryId, 10, 20, 30, 40, 50, 7)
            };
        var rng = new Random(10);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Intersect(result.BonusNumbers)
            .Should().BeEmpty();
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_Non_Negative_Confidence()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, 1, 2, 3, 4, 5),
                Draw(config.LotteryId, 10, 20, 30, 35, 40)
            };
        var rng = new Random(11);
        var sut = new GroupSelectionAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeGreaterThanOrEqualTo(0d);
    }

    private static LotteryConfigurationDomain CreateConfig(
        int mainCount = 5,
        int mainRange = 50,
        int bonusCount = 2,
        int bonusRange = 10,
        int lotteryId = 777)
        => new()
        {
            LotteryId = lotteryId,
            MainNumbersCount = mainCount,
            MainNumbersRange = mainRange,
            BonusNumbersCount = bonusCount,
            BonusNumbersRange = bonusRange
        };
}