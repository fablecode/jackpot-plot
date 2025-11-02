using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.CyclicPatternsAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_Empty_History_When_Predict_Method_Is_Invoked_Should_Return_Empty_PredictedNumbers_And_Bonus()
    {
        // Arrange
        var sut = new CyclicPatternsAlgorithm();
        var resultConfig = Config(mainCount: 5, bonusCount: 2);
        var rng = new Random(123);

        // Act
        var result = sut.Predict(resultConfig, Array.Empty<HistoricalDraw>(), rng);

        // Assert
        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty).Should().BeTrue(); // early-return path
    }

    [Test]
    public void Given_Valid_History_When_Predict_Method_Is_Invoked_Should_Set_AlgorithmKey_To_CyclicPatterns()
    {
        // Arrange
        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config();
        var history = new List<HistoricalDraw> { Draw(DateTime.UtcNow, 1, 2, 3) };
        var rng = new Random(1);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.AlgorithmKey.Should().Be(PredictionAlgorithmKeys.CyclicPatterns);
    }

    [Test]
    public void Given_Valid_History_When_Predict_Method_Is_Invoked_Should_Preserve_LotteryId()
    {
        // Arrange
        var lotteryId = 5;
        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(lotteryId: lotteryId);
        var history = new List<HistoricalDraw> { Draw(DateTime.UtcNow, 7, 8) };
        var rng = new Random(2);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.LotteryId.Should().Be(lotteryId);
    }

    [Test]
    public void Given_BonusCount_Zero_When_Predict_Method_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(bonusCount: 0);
        var history = new List<HistoricalDraw> { Draw(DateTime.UtcNow, 1, 2, 3) };
        var rng = new Random(3);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Positive_BonusCount_When_Predict_Method_Is_Invoked_Bonus_Count_Should_Equal_Config()
    {
        // Arrange
        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(bonusCount: 3, bonusRange: 12);
        var history = new List<HistoricalDraw> { Draw(DateTime.UtcNow, 1, 2, 3) };
        var rng = new Random(4);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(cfg.BonusNumbersCount);
    }

    [Test]
    public void Given_Bonus_Generation_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range_And_Exclude_Main()
    {
        // Arrange
        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(mainRange: 20, mainCount: 3, bonusRange: 10, bonusCount: 4);
        var history = new List<HistoricalDraw>
            {
                Draw(DateTime.UtcNow.AddDays(-2), 1,2,3,4),
                Draw(DateTime.UtcNow.AddDays(-1), 5,6,7),
                Draw(DateTime.UtcNow, 8,9,10)
            };
        var rng = new Random(5);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        (result.BonusNumbers.All(b => b >= 1 && b <= cfg.BonusNumbersRange) &&
         !result.BonusNumbers.Intersect(result.PredictedNumbers).Any()).Should().BeTrue();
    }

    // ----------- Behavior derived from AnalyzeCyclicPatterns + GenerateNumbersFromCycles -----------

    [Test]
    public void Given_Repeated_Appearances_When_Predict_Method_Is_Invoked_Should_Prefer_Shortest_Average_Cycle()
    {
        // Arrange
        // Construct cycles where:
        //  - 7 appears in draws 0,1,2 → gaps [1,1] → average 1 (very short)
        //  - 3 appears in draws 0,3     → gaps [3]
        //  - 10 appears in draws 1,4    → gaps [3]
        //  - 20 appears only once       → gaps []
        var start = new DateTime(2024, 1, 1);
        var history = new List<HistoricalDraw>
            {
                Draw(start.AddDays(0), 7, 3, 20),
                Draw(start.AddDays(1), 7, 10),
                Draw(start.AddDays(2), 7),
                Draw(start.AddDays(3), 3),
                Draw(start.AddDays(4), 10),
            };

        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(mainRange: 30, mainCount: 1); // force a single pick
        var rng = new Random(42);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Single().Should().Be(7); // shortest avg cycle should win before shuffle
    }

    [Test]
    public void Given_No_Number_With_Two_Appearances_When_Predict_Method_Is_Invoked_Should_Return_Empty_Main()
    {
        // Arrange
        // Each number appears at most once → every gaps list is empty → cycles ordering yields zero candidates.
        var start = new DateTime(2024, 1, 1);
        var history = new List<HistoricalDraw>
            {
                Draw(start.AddDays(0), 1, 4, 7),
                Draw(start.AddDays(1), 2, 5, 8),
                Draw(start.AddDays(2), 3, 6, 9),
            };

        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 5);
        var rng = new Random(7);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(0); // exercises GenerateNumbersFromCycles filtering
    }

    // ----------- Confidence (CalculateCyclicConfidence) -----------

    [Test]
    public void Given_Predicted_Number_Is_Due_By_Average_When_Predict_Method_Is_Invoked_Should_Return_ConfidenceScore_One_With_Single_PredictedNumbers()
    {
        // Arrange
        // 7 last appeared at index 2; history count = 5; distanceSinceLast = 4 - 2 = 2
        // gaps for 7: [1,1] → avg = 1 → distanceSinceLast >= avg → due
        var start = new DateTime(2024, 1, 1);
        var history = new List<HistoricalDraw>
            {
                Draw(start.AddDays(0), 7),
                Draw(start.AddDays(1), 7),
                Draw(start.AddDays(2), 7),
                Draw(start.AddDays(3), 3),
                Draw(start.AddDays(4), 10),
            };

        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(mainRange: 30, mainCount: 1);
        var rng = new Random(99);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(1.0);
    }

    [Test]
    public void Given_PredictionNumbers_Is_Empty_When_Predict_Method_Is_Invoked_Should_Return_ConfidenceScore_Zero()
    {
        // Arrange
        // No cycles → main empty → confidence path uses predicted.Length==0 → 0
        var start = new DateTime(2024, 1, 1);
        var history = new List<HistoricalDraw>
            {
                Draw(start.AddDays(0), 1, 2),
                Draw(start.AddDays(1), 3, 4),
                Draw(start.AddDays(2), 5, 6),
            };

        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(mainRange: 10, mainCount: 5);
        var rng = new Random(101);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.ConfidenceScore.Should().Be(0.0);
    }

    // ----------- Ranges & distinctness (GenerateRandomNumbers) -----------

    [Test]
    public void Given_BonusRange_When_Predict_Method_Is_Invoked_Should_Return_Bonus_In_Range()
    {
        // Arrange
        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(bonusRange: 7, bonusCount: 3);
        var history = new List<HistoricalDraw> { Draw(DateTime.UtcNow, 1, 2, 3) };
        var rng = new Random(11);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.All(n => n >= 1 && n <= cfg.BonusNumbersRange).Should().BeTrue();
    }

    [Test]
    public void Given_Positive_BonusCount_When_Predict_Method_Is_Invoked_Should_Return_Distinct_Bonus()
    {
        // Arrange
        var sut = new CyclicPatternsAlgorithm();
        var cfg = Config(bonusRange: 12, bonusCount: 5);
        var history = new List<HistoricalDraw> { Draw(DateTime.UtcNow, 4, 5, 6) };
        var rng = new Random(12);

        // Act
        var result = sut.Predict(cfg, history, rng);

        // Assert
        result.BonusNumbers.Distinct().Count().Should().Be(result.BonusNumbers.Length);
    }

    // ----------- Helpers -----------

    private static HistoricalDraw Draw(DateTime date, params int[] main) =>
        new HistoricalDraw(
            DrawId: date.Day,  // arbitrary
            LotteryId: 1,
            DrawDate: date,
            WinningNumbers: main.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: date);

    private static LotteryConfigurationDomain Config(
        int lotteryId = 8,
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

    // ----------- Core shape & invariants (Predict integrates all helpers) -----------

}