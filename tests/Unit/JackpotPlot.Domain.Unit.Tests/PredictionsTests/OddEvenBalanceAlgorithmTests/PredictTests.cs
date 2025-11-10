using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.OddEvenBalanceAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_OddEvenBalance()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3) };
        var rng = new Random(1);

        var result = sut.Predict(cfg, history, rng);

        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.OddEvenBalance);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        const int lid = 12;
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(lotteryId: lid);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4) };
        var rng = new Random(2);

        var result = sut.Predict(cfg, history, rng);

        result.LotteryId.Should().Be(lid);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_ConfidenceScore_Should_Be_Zero()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainCount: 6);
        var rng = new Random(3);

        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        result.ConfidenceScore.Should().Be(0.0);
    }

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equal_Config()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainCount: 5);
        var rng = new Random(4);

        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_PredictedNumbers_Count_Should_Equal_Config()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3, 4, 5) };
        var rng = new Random(5);

        var result = sut.Predict(cfg, history, rng);

        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_PredictedNumbers_In_Range()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 5);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(6);

        var result = sut.Predict(cfg, history, rng);

        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictedNumbers()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6);
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3, 4, 5) };
        var rng = new Random(7);

        var result = sut.Predict(cfg, history, rng);

        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    [Test]
    public void Given_Slight_Odd_Dominance_When_Predict_Method_Is_Invoked_Should_Round_OddCount_Up()
    {
        // history has slightly more odds than evens overall → oddRatio just over 0.5
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainRange: 50, mainCount: 5);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1,3,5,10), // 3 odds, 1 even
                Draw(2, 7,9,12),   // 2 odds, 1 even
                Draw(3, 2,4,6,11)  // 1 odd, 3 evens
            };
        var rng = new Random(8);

        var result = sut.Predict(cfg, history, rng);

        result.PredictedNumbers.Count(n => (n & 1) == 1).Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public void Given_BonusDisabled_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 1, 2, 3) };
        var rng = new Random(9);

        var result = sut.Predict(cfg, history, rng);

        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(10);

        var result = sut.Predict(cfg, history, rng);

        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_BonusEnabled_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 6, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3, 4, 5) };
        var rng = new Random(11);

        var result = sut.Predict(cfg, history, rng);

        result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_Balanced_History_When_Predict_Method_Is_Invoked_Should_Return_ConfidenceScore_Of_One()
    {
        var sut = new OddEvenBalanceAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 4, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(1, 1,2,3,4), // 2 odd, 2 even
                Draw(2, 5,6,7,8), // 2 odd, 2 even
            };
        var rng = new Random(12);

        var result = sut.Predict(cfg, history, rng);

        result.ConfidenceScore.Should().Be(1.0);
    }

    // ---------- helpers ----------
    private static HistoricalDraw Draw(int id, params int[] main) =>
        new(
            DrawId: id,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow.AddDays(id),
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow.AddDays(id));

    private static LotteryConfigurationDomain Config(
        int lotteryId = 13,
        int mainRange = 50,
        int mainCount = 6,
        int bonusRange = 12,
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