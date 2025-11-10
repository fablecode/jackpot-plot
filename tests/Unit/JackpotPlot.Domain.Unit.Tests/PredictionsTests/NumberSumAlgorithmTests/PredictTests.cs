using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.NumberSumAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_NumberSum()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3) };
        var rng = new Random(1);

        var result = sut.Predict(cfg, history, rng);

        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.NumberSum);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        var lid = new Random(12345).Next();
        var sut = new NumberSumAlgorithm();
        var cfg = Config(lotteryId: lid);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4) };
        var rng = new Random(2);

        var result = sut.Predict(cfg, history, rng);

        result.LotteryId.Should().Be(lid);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_Empty_PredictedNumbers_And_Bonus_And_Confidence_Zero()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config(mainCount: 5, bonusCount: 2);
        var rng = new Random(3);

        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty && result.ConfidenceScore == 0.0)
            .Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equal_Config()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config(mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3, 4, 5) };
        var rng = new Random(4);

        var result = sut.Predict(cfg, history, rng);

        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Should_Be_In_Range()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 5);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(5);

        var result = sut.Predict(cfg, history, rng);

        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictedNumbers()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1, 2, 3),
                Draw(2, 2, 3, 4),
            };
        var rng = new Random(6);

        var result = sut.Predict(cfg, history, rng);

        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3) };
        var rng = new Random(7);

        var result = sut.Predict(cfg, history, rng);

        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(8);

        var result = sut.Predict(cfg, history, rng);

        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range_And_Not_Overlap_PredictedNumbers()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 5, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(9);

        var result = sut.Predict(cfg, history, rng);

        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         !result.BonusNumbers.Intersect(result.PredictedNumbers).Any()).Should().BeTrue();
    }

    [Test]
    public void Given_Common_Sums_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_Greater_Than_Zero()
    {
        var sut = new NumberSumAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 4, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1, 2, 3, 4),
                Draw(2, 2, 3, 4, 5),
                Draw(3, 3, 4, 5, 6)
            };
        var rng = new Random(10);

        var result = sut.Predict(cfg, history, rng);

        result.ConfidenceScore.Should().BeGreaterThan(0.0);
    }

    [Test]
    public void Given_No_History_And_Even_MainCount_When_Predict_Method_Is_Invoked_Should_Return_Half_Odds_Half_Evens()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainRange: 50, mainCount: 6);
        var rng = new Random(13);

        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        result.PredictedNumbers.Count(n => (n & 1) == 1).Should().Be(cfg.MainNumbersCount / 2);
    }

    [Test]
    public void Given_No_History_And_BonusEnabled_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainRange: 50, mainCount: 5, bonusRange: 12, bonusCount: 3);
        var rng = new Random(14);

        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    private static HistoricalDraw Draw(int id, params int[] main) =>
        new(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow.AddDays(id));

    private static LotteryConfigurationDomain Config(
        int lotteryId = 2,
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