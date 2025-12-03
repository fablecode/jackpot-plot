using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Algorithms;
using JackpotPlot.Domain.Predictions.Helpers;
using JackpotPlot.Primitives.Algorithms;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.SeasonalPatternsAlgorithmTests;

[TestFixture]
public class PredictTests
{
    private static DateTime ADateIn(string season)
    {
        // Keep dates UTC and deterministic per season bucket used by helpers.
        return season switch
        {
            "Winter" => new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            "Spring" => new DateTime(2025, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            "Summer" => new DateTime(2025, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            _ => new DateTime(2025, 10, 15, 0, 0, 0, DateTimeKind.Utc), // Fall
        };
    }

    private static string CurrentSeason() => SeasonalPatternsAlgorithmHelpers.GetSeason(DateTime.UtcNow);

    [Test]
    public void Given_No_History_When_Predict_Is_Invoked_Should_Return_Empty_And_Zero_Confidence()
    {
        // Arrange
        var config = CreateConfig();
        var history = Array.Empty<HistoricalDraw>();
        var rng = new Random(42);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        (result.PredictedNumbers.IsEmpty && result.BonusNumbers.IsEmpty && result.ConfidenceScore == 0d)
            .Should().BeTrue();
    }

    [Test]
    public void Given_Seasonal_Repeats_When_Predict_Is_Invoked_Should_Include_Most_Frequent_Seasonal_Number()
    {
        // Arrange
        var season = CurrentSeason();
        var d1 = ADateIn(season);
        var d2 = ADateIn(season).AddDays(7);
        var d3 = ADateIn(season).AddDays(14);

        // Make number 7 the unique top frequency in the current season
        var history = new List<HistoricalDraw>
            {
                Draw(d1, new[] { 7, 1, 2, 3 }),
                Draw(d2, new[] { 7, 4, 5 }),
                Draw(d3, new[] { 7, 8, 9 }),
                // Add off-season noise that shouldn't influence seasonal pick
                Draw(d3.AddMonths(3), new[] { 1, 1, 1 })
            };

        var config = CreateConfig(mainCount: 3, mainRange: 50, bonusCount: 0);
        var rng = new Random(7);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().Contain(7);
    }

    [Test]
    public void Given_Sparse_Seasonal_Data_When_Predict_Is_Invoked_Should_Top_Up_To_MainNumbersCount()
    {
        // Arrange
        var season = CurrentSeason();
        var d1 = ADateIn(season);

        // Only one seasonal number occurs; rest must be random-filled
        var history = new List<HistoricalDraw>
            {
                Draw(d1, new[] { 11 })
            };

        var config = CreateConfig(mainCount: 5, mainRange: 60, bonusCount: 0);
        var rng = new Random(42);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Length.Should().Be(5);
    }

    [Test]
    public void Given_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Requested_Bonus_Count()
    {
        // Arrange
        var season = CurrentSeason();
        var history = new List<HistoricalDraw>
            {
                Draw(ADateIn(season), new[] { 2, 3, 4 })
            };

        var config = CreateConfig(mainCount: 4, mainRange: 40, bonusCount: 3, bonusRange: 10);
        var rng = new Random(123);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Length.Should().Be(3);
    }

    [Test]
    public void Given_No_Bonus_Config_When_Predict_Is_Invoked_Should_Return_Empty_Bonus()
    {
        // Arrange
        var season = CurrentSeason();
        var history = new List<HistoricalDraw>
            {
                Draw(ADateIn(season), new[] { 2, 3, 4 })
            };

        var config = CreateConfig(mainCount: 4, mainRange: 40, bonusCount: 0, bonusRange: 0);
        var rng = new Random(5);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_History_When_Predict_Is_Invoked_Should_Set_LotteryId_And_Key()
    {
        // Arrange
        var season = CurrentSeason();
        var history = new List<HistoricalDraw>
            {
                Draw(ADateIn(season), new[] { 10, 20 })
            };

        var config = CreateConfig(lotteryId: 777);
        var rng = new Random(9);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        (result.LotteryId == 777 && result.AlgorithmKey == PredictionAlgorithmKeys.SeasonalPatterns)
            .Should().BeTrue();
    }

    [Test]
    public void Given_Seasonal_Draws_When_Predict_Is_Invoked_Should_Compute_Positive_Confidence_When_Overlap_Exists()
    {
        // Arrange
        var season = CurrentSeason();
        var d1 = ADateIn(season);
        var d2 = d1.AddDays(7);

        // Number 5 appears in both seasonal draws, making overlap likely
        var history = new List<HistoricalDraw>
            {
                Draw(d1, new[] { 5, 1, 2 }),
                Draw(d2, new[] { 5, 3, 4 })
            };

        var config = CreateConfig(mainCount: 3, mainRange: 20, bonusCount: 0);
        var rng = new Random(11);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.ConfidenceScore.Should().BeGreaterThan(0d);
    }

    [Test]
    public void Given_Config_Ranges_When_Predict_Is_Invoked_Should_Keep_Main_Numbers_In_Range()
    {
        // Arrange
        var season = CurrentSeason();
        var history = new List<HistoricalDraw>
            {
                Draw(ADateIn(season), new[] { 1, 2, 3 })
            };

        var config = CreateConfig(mainCount: 6, mainRange: 12, bonusCount: 0);
        var rng = new Random(22);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().OnlyContain(n => n >= 1 && n <= 12);
    }

    [Test]
    public void Given_Config_Ranges_When_Predict_Is_Invoked_Should_Keep_Bonus_Numbers_In_Range()
    {
        // Arrange
        var season = CurrentSeason();
        var history = new List<HistoricalDraw>
            {
                Draw(ADateIn(season), new[] { 1, 2, 3 })
            };

        var config = CreateConfig(mainCount: 4, mainRange: 40, bonusCount: 2, bonusRange: 5);
        var rng = new Random(33);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.BonusNumbers.Should().OnlyContain(n => n >= 1 && n <= 5);
    }

    [Test]
    public void Given_Sparse_Seasonal_Data_When_Predict_Is_Invoked_Should_Produce_Distinct_Predicted_Numbers()
    {
        // Arrange
        var season = CurrentSeason();
        var history = new List<HistoricalDraw>
        {
            // Still sparse; random filler must be distinct from each other and from any seeded seasonal picks
            Draw(ADateIn(season), new[] { 5 })
        };
        var config = CreateConfig(mainCount: 7, mainRange: 45);
        var rng = new Random(7);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert
        result.PredictedNumbers.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void Given_Too_Sparse_Seasonal_Data_When_Predict_Is_Invoked_Should_Top_Up_Randomly()
    {
        // Arrange — only one draw, giving just one seasonal number
        var config = CreateConfig(mainCount: 6, mainRange: 40);
        var history = new List<HistoricalDraw>
        {
            Draw(ADateInCurrentSeason(), [1])
        };
        var rng = new Random(42);
        var sut = new SeasonalPatternsAlgorithm();

        // Act
        var result = sut.Predict(config, history, rng);

        // Assert — main numbers should be topped up to full count
        result.PredictedNumbers.Length.Should().Be(6);
        result.PredictedNumbers.Should().OnlyHaveUniqueItems();
        result.PredictedNumbers.Should().Contain(1); // original seasonal number still present
    }

    private static DateTime ADateInCurrentSeason()
    {
        var season = SeasonalPatternsAlgorithmHelpers.GetSeason(DateTime.UtcNow);
        return season switch
        {
            "Winter" => new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            "Spring" => new DateTime(2025, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            "Summer" => new DateTime(2025, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            _ => new DateTime(2025, 10, 15, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private static LotteryConfigurationDomain CreateConfig(
        int mainCount = 5,
        int mainRange = 50,
        int bonusCount = 2,
        int bonusRange = 10,
        int lotteryId = 101)
        => new()
        {
            LotteryId = lotteryId,
            MainNumbersCount = mainCount,
            MainNumbersRange = mainRange,
            BonusNumbersCount = bonusCount,
            BonusNumbersRange = bonusRange
        };

    private static HistoricalDraw Draw(DateTime dateUtc, IEnumerable<int> main, IEnumerable<int>? bonus = null, int lotteryId = 101)
        => new(
            DrawId: 1,
            LotteryId: lotteryId,
            DrawDate: dateUtc,
            WinningNumbers: main.ToList(),
            BonusNumbers: (bonus ?? Array.Empty<int>()).ToList(),
            CreatedAt: dateUtc
        );



}