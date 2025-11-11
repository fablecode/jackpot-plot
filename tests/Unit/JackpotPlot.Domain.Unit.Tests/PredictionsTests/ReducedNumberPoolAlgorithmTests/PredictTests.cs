using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.ReducedNumberPoolAlgorithmTests;

[TestFixture]
public class PredictTests
{
    private static LotteryConfigurationDomain CreateConfig(
            int mainCount = 5,
            int mainRange = 50,
            int bonusCount = 2,
            int bonusRange = 10,
            int lotteryId = 7)
            => new()
            {
                LotteryId = lotteryId,
                MainNumbersCount = mainCount,
                MainNumbersRange = mainRange,
                BonusNumbersCount = bonusCount,
                BonusNumbersRange = bonusRange
            };

    private static HistoricalDraw CreateDraw(IEnumerable<int> main, IEnumerable<int> bonus, int lotteryId = 7)
        => new(
            DrawId: 1,
            LotteryId: lotteryId,
            DrawDate: new DateTime(2024, 1, 1),
            WinningNumbers: main.ToList(),
            BonusNumbers: bonus.ToList(),
            CreatedAt: new DateTime(2024, 1, 2));

    [Test]
    public void Given_Empty_History_When_Predict_Is_Invoked_Should_Produce_PredictedNumbers_Count()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 40, bonusCount: 0);
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(6);
    }

    [Test]
    public void Given_Empty_History_When_Predict_Is_Invoked_Should_Produce_BonusNumbers_Count()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 3, bonusRange: 12);
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(3);
    }

    [Test]
    public void Given_Empty_History_When_Predict_Is_Invoked_Should_Set_LotteryId()
    {
        // Arrange
        var config = CreateConfig(lotteryId: 1234);
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.LotteryId.Should().Be(1234);
    }

    [Test]
    public void Given_Empty_History_When_Predict_Is_Invoked_Should_Set_AlgorithmKey()
    {
        // Arrange
        var config = CreateConfig();
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.ReducedNumberPool);
    }

    [Test]
    public void Given_Empty_History_When_Predict_Is_Invoked_Should_Generate_PredictedNumbers_Within_Range()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 35, bonusCount: 0);
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= 35);
    }

    [Test]
    public void Given_Empty_History_When_Predict_Is_Invoked_Should_Generate_BonusNumbers_Within_Range()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 35, bonusCount: 2, bonusRange: 9);
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= 9);
    }

    [Test]
    public void Given_Empty_History_When_Predict_Is_Invoked_Should_Not_Duplicate_Between_PredictedNumbers_And_Bonus()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 40, bonusCount: 3, bonusRange: 15);
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Intersect(result.BonusNumbers).Should().BeEmpty();
    }

    [Test]
    public void Given_High_Threshold_And_Small_Common_Pool_When_Predict_Is_Invoked_Should_Fill_From_Full_Range_To_PredictedNumbers_Count()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 30, bonusCount: 0);
        var history = new[]
        {
                CreateDraw([1,2,3,4,5], Array.Empty<int>()),
                CreateDraw([1,6,7,8,9], Array.Empty<int>())
            };
        var sut = new ReducedNumberPoolAlgorithm(appearanceThresholdRatio: 1.0);
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(5);
    }

    [Test]
    public void Given_No_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 50, bonusCount: 0, bonusRange: 0);
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Empty_History_When_Predict_Is_Invoked_Should_Return_Zero_ConfidenceScore()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 0);
        var history = Array.Empty<HistoricalDraw>();
        var sut = new ReducedNumberPoolAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(0d);
    }
}