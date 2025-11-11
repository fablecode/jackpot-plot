using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.StandardDeviationAlgorithmTests;

[TestFixture]
public class PredictTests
{
    private static LotteryConfigurationDomain CreateConfig(
            int mainCount = 5,
            int mainRange = 50,
            int bonusCount = 2,
            int bonusRange = 10,
            int lotteryId = 888)
            => new()
            {
                LotteryId = lotteryId,
                MainNumbersCount = mainCount,
                MainNumbersRange = mainRange,
                BonusNumbersCount = bonusCount,
                BonusNumbersRange = bonusRange
            };

    private static HistoricalDraw Draw(IEnumerable<int> main, IEnumerable<int>? bonus = null, int lotteryId = 888)
        => new(
            DrawId: 1,
            LotteryId: lotteryId,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.ToList(),
            BonusNumbers: (bonus ?? Array.Empty<int>()).ToList(),
            CreatedAt: DateTime.UtcNow
        );

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_And_Zero_Confidence()
    {
        // Arrange
        var config = CreateConfig();
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(42);
        var sut = new StandardDeviationAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty && result.ConfidenceScore == 0d)
            .Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Set_LotteryId_And_Key()
    {
        // Arrange
        var config = CreateConfig(lotteryId: 999);
        var history = new List<HistoricalDraw> { Draw([1, 2, 3, 4, 5]) };
        var rng = new Random(42);
        var sut = new StandardDeviationAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.LotteryId.Should().Be(999);
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.StandardDeviation);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_PredictedNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 40, bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw([1, 2, 3, 4, 5, 6]) };
        var rng = new Random(7);
        var sut = new StandardDeviationAlgorithm();

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
        var history = new List<HistoricalDraw> { Draw([5, 10, 15, 20, 25]) };
        var rng = new Random(11);
        var sut = new StandardDeviationAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= 30);
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_BonusNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 3, bonusRange: 15);
        var history = new List<HistoricalDraw> { Draw([1, 3, 5, 7, 9]) };
        var rng = new Random(19);
        var sut = new StandardDeviationAlgorithm();

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
        var history = new List<HistoricalDraw> { Draw([1, 2, 3, 4, 5]) };
        var rng = new Random(42);
        var sut = new StandardDeviationAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Results_When_Predict_Is_Invoked_Should_Not_Overlap_Between_PredictedNumbers_And_Bonus()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 50, bonusCount: 2, bonusRange: 10);
        var history = new List<HistoricalDraw> { Draw([5, 10, 15, 20, 25]) };
        var rng = new Random(33);
        var sut = new StandardDeviationAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Intersect(result.BonusNumbers).Should().BeEmpty();
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_Positive_Confidence()
    {
        // Arrange
        var config = CreateConfig(mainCount: 5, mainRange: 40, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw([1, 2, 3, 4, 5]),
                Draw([6, 7, 8, 9, 10])
            };
        var rng = new Random(55);
        var sut = new StandardDeviationAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeGreaterThanOrEqualTo(0d);
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Return_PredictedNumbers_Unique()
    {
        // Arrange
        var config = CreateConfig(mainCount: 6, mainRange: 45, bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw([1, 2, 3, 4, 5, 6]) };
        var rng = new Random(99);
        var sut = new StandardDeviationAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().OnlyHaveUniqueItems();
    }
}