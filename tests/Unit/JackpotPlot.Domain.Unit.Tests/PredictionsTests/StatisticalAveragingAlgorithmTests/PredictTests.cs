using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.StatisticalAveragingAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_PredictedNumbers()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Zero_Confidence()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().Be(0d);
    }


    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Set_LotteryId()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(lotteryId: 777);
        var history = new List<HistoricalDraw> { Draw([1, 2, 3]) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.LotteryId.Should().Be(777);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Set_AlgorithmKey()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig();
        var history = new List<HistoricalDraw> { Draw([1, 2, 3]) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.StatisticalAveraging);
    }


    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_PredictedNumbersNumbersCount_Length()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(mainCount: 6, mainRange: 40, bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw([1, 2, 3, 4, 5, 6]) };
        var rng = new Random(2);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(6);
    }

    [Test]
    public void Given_Positional_Data_When_Predict_Is_Invoked_Should_Return_Rounded_Averages()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(mainCount: 2, mainRange: 50, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw([1, 3]),
                Draw([2, 4])
            };
        var rng = new Random(3);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().Equal(2, 4);
    }

    [Test]
    public void Given_Config_Range_When_Predict_Is_Invoked_Should_Keep_PredictedNumbers_In_Range()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(mainCount: 5, mainRange: 30, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw([5, 10, 15, 20, 35]) 
            };
        var rng = new Random(4);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= 30);
    }


    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_BonusNumbersCount_Length()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(mainCount: 4, mainRange: 40, bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw([1, 2, 3, 4], [5, 7, 9]) };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(3);
    }

    [Test]
    public void Given_IsBonus_Averages_When_Predict_Is_Invoked_Should_Return_Rounded_Bonus_Averages()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(mainCount: 2, mainRange: 50, bonusCount: 2, bonusRange: 50);
        var history = new List<HistoricalDraw>
            {
                Draw([1, 2], [5, 7]),
                Draw([3, 4], [7, 9])
            };
        var rng = new Random(6);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Should().Equal(6, 8);
    }

    [Test]
    public void Given_No_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 0, bonusRange: 0);
        var history = new List<HistoricalDraw> { Draw([1, 2, 3, 4, 5]) };
        var rng = new Random(7);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Bonus_Range_When_Predict_Is_Invoked_Should_Keep_Bonus_In_Range()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(mainCount: 4, mainRange: 40, bonusCount: 2, bonusRange: 7);
        var history = new List<HistoricalDraw>
            {
                Draw([1, 2, 3, 4], [3, 6])
            };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= 7);
    }


    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_Non_Negative_Confidence()
    {
        // Arrange
        var sut = new StatisticalAveragingAlgorithm();
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw([2, 2, 2, 2, 2]),
                Draw([3, 3, 3, 3, 3])
            };
        var rng = new Random(9);

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
        int lotteryId = 123)
        => new()
        {
            LotteryId = lotteryId,
            MainNumbersCount = mainCount,
            MainNumbersRange = mainRange,
            BonusNumbersCount = bonusCount,
            BonusNumbersRange = bonusRange
        };

    private static HistoricalDraw Draw(IEnumerable<int> main, IEnumerable<int>? bonus = null, int lotteryId = 123)
        => new(
            DrawId: 1,
            LotteryId: lotteryId,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: (bonus ?? Array.Empty<int>()).ToList(),
            CreatedAt: DateTime.UtcNow
        );
}