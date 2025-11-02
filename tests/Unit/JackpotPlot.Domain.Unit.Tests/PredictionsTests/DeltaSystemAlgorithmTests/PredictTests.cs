using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.DeltaSystemAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_Valid_Config_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_DeltaSystem()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.DeltaSystem);
    }

    [Test]
    public void Given_Valid_Config_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        const int lotteryId = 3;
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(lotteryId: lotteryId);
        var history = new List<HistoricalDraw> { Draw(10, 20, 30) };
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    // ---------- no-history fallback path ----------
    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_ConfidenceScore_Zero()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainCount: 5);
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.ConfidenceScore.Should().Be(0.0);
    } // follows the early random-fill path when allDeltas is empty. :contentReference[oaicite:2]{index=2}

    [Test]
    public void Given_No_History_When_Predict_Method_Is_Invoked_Should_Return_PredictionNumbers_Count_Equals_Config()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainCount: 6, mainRange: 40);
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    }

    [Test]
    public void Given_History_With_No_Deltas_When_Predict_Method_Is_Invoked_Should_Use_Fallback_Random_PredictionNumbers()
    {
        // Arrange
        // Each draw has a single number → CalculateDeltas returns empty → fallback path.
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainCount: 4, mainRange: 30);
        var history = new List<HistoricalDraw> { Draw(5), Draw(12), Draw(29) };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(4);
    }

    // ---------- delta-driven path ----------
    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_PredictionNumbers_Count_Should_Equals_Config()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 5);
        var history = new List<HistoricalDraw> { Draw(2, 3, 5, 8), Draw(10, 11, 12) };
        var rng = new Random(6);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(cfg.MainNumbersCount);
    } 

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_PredictionNumbers_In_Range()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainRange: 18, mainCount: 5);
        var history = new List<HistoricalDraw> { Draw(2, 4, 8, 16) };
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.All(n => n >= 1 && n <= cfg.MainNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Method_Is_Invoked_Should_Return_Distinct_PredictionNumbers()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainRange: 30, mainCount: 6);
        var history = new List<HistoricalDraw>
            {
                Draw(1,3,6,10),
                Draw(2,5,9),
                Draw(12,15,19)
            };
        var rng = new Random(8);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Distinct().Count().Should().Be(result.PredictedNumbers.Length);
    }

    // ---------- bonus generation ----------
    [Test]
    public void Given_Bonus_Count_Zero_When_Predict_Method_Is_Invoked_Should_Return_Empty_BonusNumbers()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(1, 2, 3) };
        var rng = new Random(9);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Positive_BonusCount_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(1, 3, 6, 10) };
        var rng = new Random(10);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    } // generated via RandomDistinct with exclude = main. :contentReference[oaicite:5]{index=5}

    [Test]
    public void Given_Bonus_Generation_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range_And_Not_Overlap_Main()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainRange: 40, mainCount: 5, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw> { Draw(5, 9, 14, 20, 27) };
        var rng = new Random(11);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         !result.BonusNumbers.Intersect(result.PredictedNumbers).Any()).Should().BeTrue();
    }

    // ---------- confidence ----------
    [Test]
    public void Given_PredictionNumbers_Count_One_When_Predict_Method_Is_Invoked_Should_Return_ConfidenceScore_Of_Zero()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainRange: 30, mainCount: 1);
        var history = new List<HistoricalDraw> { Draw(2, 3, 4) };
        var rng = new Random(12);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(0.0);
    }

    [Test]
    public void Given_Common_Delta_Patterns_When_Predict_Method_Is_Invoked_Should_Return_ConfidenceScore_Greater_Than_Zero()
    {
        // Arrange
        var sut = new DeltaSystemAlgorithm();
        var cfg = Config(mainRange: 25, mainCount: 4, bonusCount: 0);
        var history = new List<HistoricalDraw>
            {
                Draw(1,2,3,4),
                Draw(6,7,8),
                Draw(10,11,12,13),
                Draw(15,16,17)
            };
        var rng = new Random(13);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeGreaterThan(0.0);
    }

    // ---------- helpers ----------
    private static HistoricalDraw Draw(params int[] main) =>
        new(
            DrawId: 1,
            LotteryId: 1,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: main.OrderBy(n => n).ToList(), // deltas assume ascending
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow);

    private static LotteryConfigurationDomain Config(
        int lotteryId = 9,
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

    // ---------- metadata ----------

}