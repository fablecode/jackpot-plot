using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.RepeatingNumbersAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_PredictedNumbers()
    {
        // Arrange
        var config = CreateConfig();
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(42);
        var sut = new RepeatingNumbersAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        var config = CreateConfig();
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(42);
        var sut = new RepeatingNumbersAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Zero_Confidence()
    {
        // Arrange
        var config = CreateConfig();
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(42);
        var sut = new RepeatingNumbersAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(0d);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Set_LotteryId()
    {
        // Arrange
        var config = CreateConfig(lotteryId: 999);
        var history = new List<HistoricalDraw>
            {
                Draw(999, new DateTime(2024, 1, 1), 1, 2, 3)
            };
        var rng = new Random(1);
        var sut = new RepeatingNumbersAlgorithm();

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
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, new DateTime(2024, 1, 1), 1, 2, 3)
            };
        var rng = new Random(1);
        var sut = new RepeatingNumbersAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.RepeatingNumbers);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_PredictedNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 40, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(config.LotteryId, new DateTime(2024, 1, 5), 1, 1, 2, 3),
                Draw(config.LotteryId, new DateTime(2024, 1, 4), 2, 2, 3, 4)
            };
        var rng = new Random(2);
        var sut = new RepeatingNumbersAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(6);
    }

    [Test]
    public void Given_RecentDrawsToConsider_One_When_Predict_Is_Invoked_Should_Use_Only_Most_Recent_Draw()
    {
        // Arrange
        var config = CreateConfig(mainCount: 1, mainRange: 50, bonusCount: 0);
        var lotteryId = config.LotteryId;

        // history newest-first
        var newest = Draw(lotteryId, new DateTime(2024, 1, 10), 7, 7, 7);
        var older = Draw(lotteryId, new DateTime(2024, 1, 1), 9, 9, 9);
        var history = new List<HistoricalDraw> { newest, older };

        var rng = new Random(3);
        var sut = new RepeatingNumbersAlgorithm(recentDrawsToConsider: 1);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Single().Should().Be(7);
    }

    [Test]
    public void Given_Sparse_Repeats_When_Predict_Is_Invoked_Should_Top_Up_Randomly_To_PredictedNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 30, bonusCount: 0);
        var lotteryId = config.LotteryId;

        var history = new List<HistoricalDraw>
            {
                Draw(lotteryId, new DateTime(2024, 1, 10), 1, 2, 3, 4, 5),
                Draw(lotteryId, new DateTime(2024, 1, 9), 6, 7, 8, 9, 10)
            };
        var rng = new Random(4);
        var sut = new RepeatingNumbersAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(5);
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_BonusNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 3, bonusRange: 12);
        var lotteryId = config.LotteryId;

        var history = new List<HistoricalDraw>
            {
                Draw(lotteryId, new DateTime(2024, 1, 10), 1, 1, 2, 2, 3)
            };
        var rng = new Random(5);
        var sut = new RepeatingNumbersAlgorithm();

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
        var lotteryId = config.LotteryId;

        var history = new List<HistoricalDraw>
            {
                Draw(lotteryId, new DateTime(2024, 1, 10), 1, 2, 3, 4, 5)
            };
        var rng = new Random(6);
        var sut = new RepeatingNumbersAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Not_Overlap_PredictNumbers_And_Bonus()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 50, bonusCount: 2, bonusRange: 15);
        var lotteryId = config.LotteryId;

        var history = new List<HistoricalDraw>
            {
                Draw(lotteryId, new DateTime(2024, 1, 10), 1, 1, 2, 2, 3),
                Draw(lotteryId, new DateTime(2024, 1, 9), 3, 3, 4, 4, 5)
            };
        var rng = new Random(7);
        var sut = new RepeatingNumbersAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Intersect(result.BonusNumbers).Should().BeEmpty();
    }

    [Test]
    public void Given_Repeating_Number_In_History_When_Predict_Is_Invoked_Should_Return_Positive_Confidence()
    {
        // Arrange
        var config = CreateConfig(mainCount: 1, mainRange: 50, bonusCount: 0);
        var lotteryId = config.LotteryId;

        var history = new List<HistoricalDraw>
            {
                Draw(lotteryId, new DateTime(2024, 1, 10), 5, 5, 5),
                Draw(lotteryId, new DateTime(2024, 1, 9), 5, 1, 2)
            };
        var rng = new Random(8);
        var sut = new RepeatingNumbersAlgorithm(recentDrawsToConsider: 10);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeGreaterThan(0d);
    }

    private static LotteryConfigurationDomain CreateConfig(
        int mainCount = 5,
        int mainRange = 50,
        int bonusCount = 2,
        int bonusRange = 10,
        int lotteryId = 1)
        => new()
        {
            LotteryId = lotteryId,
            MainNumbersCount = mainCount,
            MainNumbersRange = mainRange,
            BonusNumbersCount = bonusCount,
            BonusNumbersRange = bonusRange
        };

    private static HistoricalDraw Draw(
        int lotteryId,
        DateTime drawDate,
        params int[] main)
        => new(
            DrawId: 1,
            LotteryId: lotteryId,
            DrawDate: drawDate,
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: drawDate
        );
}