using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.NumberChainAlgorithmTests;

[TestFixture]
public class PredictTests
{
    private static Random Rng() => new(12345);

    [Test]
    public void Given_Empty_History_When_Predict_Method_Is_Invoked_Should_Return_Empty_PredictedNumbers()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config();
        var history = new List<HistoricalDraw>();

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.PredictedNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Empty_History_When_Predict_Method_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config();
        var history = new List<HistoricalDraw>();

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Empty_History_When_Predict_Method_Is_Invoked_Should_Return_Zero_Confidence()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config();
        var history = new List<HistoricalDraw>();

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.ConfidenceScore.Should().Be(0);
    }

    [Test]
    public void Given_Empty_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_NumberChain()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config();
        var history = new List<HistoricalDraw>();

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.NumberChain);
    }

    [Test]
    public void Given_Non_Empty_History_When_Predict_Method_Is_Invoked_Should_Set_LotteryId_From_Config()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config(lotteryId: 999);
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 2, 3, 4, 5) };

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.LotteryId.Should().Be(999);
    }

    [Test]
    public void Given_Non_Empty_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config(mainCount: 5);
        var history = new List<HistoricalDraw>
            {
                AlgorithmsTestHelperTests.Draw(1, 2, 3, 4, 5),
                AlgorithmsTestHelperTests.Draw(2, 3, 4, 5, 6)
            };

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.PredictedNumbers.Length.Should().Be(5);
    }

    [Test]
    public void Given_BonusCount_Zero_When_Predict_Method_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config(mainCount: 5, bonusCount: 0);
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 2, 3, 4, 5) };

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Positive_BonusCount_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config(mainCount: 5, bonusCount: 2, bonusRange: 10);
        var history = new List<HistoricalDraw>
            {
                AlgorithmsTestHelperTests.Draw(1, 2, 3, 4, 5),
                AlgorithmsTestHelperTests.Draw(2, 3, 4, 5, 6)
            };

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.BonusNumbers.Length.Should().Be(2);
    }

    [Test]
    public void Given_Non_Empty_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Should_Contain_Only_Positive_Values()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config(mainCount: 5);
        var history = new List<HistoricalDraw>
            {
                AlgorithmsTestHelperTests.Draw(1, 2, 3, 4, 5),
                AlgorithmsTestHelperTests.Draw(6, 7, 8, 9, 10)
            };

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.PredictedNumbers.All(x => x > 0).Should().BeTrue();
    }

    [Test]
    public void Given_Non_Empty_History_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_Between_0_And_1()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config(mainCount: 5);
        var history = new List<HistoricalDraw>
            {
                AlgorithmsTestHelperTests.Draw(1, 2, 3, 4, 5),
                AlgorithmsTestHelperTests.Draw(3, 4, 5, 6, 7)
            };

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.ConfidenceScore.Should().BeInRange(0, 1);
    }

    [Test]
    public void Given_Non_Empty_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_NumberChain()
    {
        // Arrange
        var sut = new NumberChainAlgorithm();
        var config = Config();
        var history = new List<HistoricalDraw> { AlgorithmsTestHelperTests.Draw(1, 2, 3, 4, 5) };

        // Act
        var result = sut.Predict(config, history, Rng());

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.NumberChain);
    }

    private static LotteryConfigurationDomain Config(
        int mainCount = 5,
        int bonusCount = 2,
        int bonusRange = 10,
        int lotteryId = 100)
    {
        return new LotteryConfigurationDomain
        {
            LotteryId = lotteryId,
            MainNumbersCount = mainCount,
            BonusNumbersCount = bonusCount,
            BonusNumbersRange = bonusRange
        };
    }
}