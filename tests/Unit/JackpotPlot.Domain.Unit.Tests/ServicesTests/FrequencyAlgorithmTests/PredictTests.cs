using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.FrequencyAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_Valid_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_FrequencyBased()
    {
        // Arrange
        var sut = new FrequencyAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.FrequencyBased);
    }

    [Test]
    public void Given_Valid_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lotteryId = 9;
        var sut = new FrequencyAlgorithm();
        var cfg = Config(lotteryId: lotteryId);
        var history = new List<HistoricalDraw> { Draw(1, 4, 5, 6) };
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Select_Lowest_N_PredictionNumbers()
    {
        // Arrange
        var sut = new FrequencyAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 4);
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, new List<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Should().Equal(1, 2, 3, 4);
    }

    [Test]
    public void Given_No_History_And_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Select_Lowest_M_Bonus()
    {
        // Arrange
        var sut = new FrequencyAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 3, bonusRange: 6, bonusCount: 2);
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, new List<HistoricalDraw>(), rng);

        // Assert
        result.BonusNumbers.Should().Equal(1, 2);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Select_Top_Frequent_PredictionNumbers_In_Descending_Order()
    {
        // Arrange
        // Frequencies: 3→4 times, 2→3 times, 1→1 time, 4→1 time
        var sut = new FrequencyAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 3);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1, 2, 2, 3),
                Draw(2, 2, 3, 3),
                Draw(3, 3, 4)
            };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Should().Equal(3, 2, 1);
    }

    // ---------- bonus selection by frequency ----------

    [Test]
    public void Given_BonusHistory_When_Predict_Method_Is_Invoked_Should_Select_Top_Frequent_Bonus()
    {
        // Arrange
        var sut = new FrequencyAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 2, bonusRange: 12, bonusCount: 1);
        var history = new List<HistoricalDraw>
            {
                DrawWithBonus(1, new[]{1,2}, 9),
                DrawWithBonus(2, new[]{2,3}, 9),
                DrawWithBonus(3, new[]{3,4}, 8)
            };
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Should().Equal(9);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new FrequencyAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 1, 2) };
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_All_Draws_Contain_Predicted_PredictionNumbers_When_Predict_Method_Is_Invoked_Should_Return_Confidence_Score_Of_One()
    {
        // Arrange
        // Ensure [1,2] are the most frequent and present in every draw.
        var sut = new FrequencyAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 2, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1,2,5),
                Draw(2, 1,2,6),
                Draw(3, 1,2,7)
            };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(1.0);
    }

    // ---------- helpers ----------
    private static HistoricalDraw Draw(int id, params int[] main) =>
        new HistoricalDraw(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow.AddDays(id));

    private static HistoricalDraw DrawWithBonus(int id, int[] main, params int[] bonus) =>
        new HistoricalDraw(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: bonus.ToList(),
            CreatedAt: DateTime.UtcNow.AddDays(id));

    private static LotteryConfigurationDomain Config(
        int lotteryId = 6,
        int mainRange = 50,
        int mainCount = 5,
        int bonusRange = 10,
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