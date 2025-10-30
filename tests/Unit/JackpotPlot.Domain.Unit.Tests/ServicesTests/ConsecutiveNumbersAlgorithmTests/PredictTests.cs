using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.ConsecutiveNumbersAlgorithmTests;

public class PredictTests
{
    [Test]
    public void Given_Valid_Config_And_Empty_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equals_Config_MainNumbersCount()
    {
        // Arrange
        var config = CreateConfig(mainRange: 60, mainCount: 6);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(123);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(config.MainNumbersCount);
    }

    [Test]
    public void Given_Bonus_Count_Zero_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus_Numbers()
    {
        // Arrange
        var config = CreateConfig(bonusCount: 0);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(123);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Positive_Bonus_Count_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equals_Config()
    {
        // Arrange
        var config = CreateConfig(bonusCount: 3, bonusRange: 12);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(123);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(config.BonusNumbersCount);
    }

    [Test]
    public void Given_Valid_Config_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_ConsecutiveNumbers()
    {
        // Arrange
        var config = CreateConfig();
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(123);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.ConsecutiveNumbers);
    }

    [Test]
    public void Given_Valid_Config_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lotteryId = 10;
        var config = CreateConfig(lotteryId: lotteryId);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(123);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    [Test]
    public void Given_MainNumbersRange_When_Predict_Method_Is_Invoked_Should_Return_PredictedNumbers_In_Range()
    {
        // Arrange
        var config = CreateConfig(mainRange: 35, mainCount: 7);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(987);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= config.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_BonusNumbersRange_When_Predict_Method_Is_Invoked_Bonus_Numbers_Should_Be_In_Range()
    {
        // Arrange
        var config = CreateConfig(bonusCount: 2, bonusRange: 9);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(555);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.All(n => n >= 1 && n <= config.BonusNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_Empty_History_When_Predict_Method_Is_Invoked_Confidence_Score_Should_Be_Zero()
    {
        // Arrange
        // With no historical consecutive pairs, total == 0 => confidence == 0 per implementation.
        var config = CreateConfig(mainRange: 50, mainCount: 5);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(123);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(0.0);
    }

    [Test]
    public void Given_Zero_PredictedNumbers_Count_When_Predict_Method_Is_Invoked_Should_Return_Zero_MainNumbers()
    {
        // Arrange
        var config = CreateConfig(mainRange: 40, mainCount: 0);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(42);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(0);
    }

    [Test]
    public void Given_Valid_Config_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictedNumbers()
    {
        // Arrange
        var config = CreateConfig(mainRange: 20, mainCount: 8);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(777);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    [Test]
    public void Given_Positive_BonusCount_When_Predict_Method_Is_Invoked_Should_Return_Distinct_BonusNumbers()
    {
        // Arrange
        var config = CreateConfig(bonusCount: 4, bonusRange: 10);
        var history = EmptyHistory();
        var sut = new ConsecutiveNumbersAlgorithm();
        var rng = new Random(888);

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Distinct().Count().Should().Be(result.BonusNumbers.Length);
    }

    #region Test Helpers

    private static LotteryConfigurationDomain CreateConfig(
        int lotteryId = 8,
        int mainRange = 50,
        int mainCount = 5,
        int bonusRange = 12,
        int bonusCount = 0)
    {
        // Adapt this factory to your actual domain ctor / setters as needed.
        return new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersRange = mainRange,
            MainNumbersCount = mainCount,
            BonusNumbersRange = bonusRange,
            BonusNumbersCount = bonusCount
        };
    }

    private static IReadOnlyList<HistoricalDraw> EmptyHistory()
    {
        // NSubstitute to satisfy the IReadOnlyList<HistoricalDraw> dependency
        var history = Substitute.For<IReadOnlyList<HistoricalDraw>>();
        history.Count.Returns(0);
        using var enumerator = history.GetEnumerator();
        using var returnThis = Enumerable.Empty<HistoricalDraw>().GetEnumerator();
        enumerator.Returns(returnThis);
        return history;
    }

    #endregion
}