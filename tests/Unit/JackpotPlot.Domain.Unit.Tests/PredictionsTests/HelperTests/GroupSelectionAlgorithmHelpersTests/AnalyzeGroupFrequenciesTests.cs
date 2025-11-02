using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.GroupSelectionAlgorithmHelpersTests;

[TestFixture]
public class AnalyzeGroupFrequenciesTests
{
    [Test]
    public void Given_Empty_History_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Return_All_Zeroes()
    {
        // Arrange
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(20, 4);

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(new List<HistoricalDraw>(), groups);

        // Assert
        freq.Values.Sum().Should().Be(0);
    }

    [Test]
    public void Given_Empty_History_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Return_All_Zeros()
    {
        // Arrange
        var groups = G((1, 5), (6, 10), (11, 15));

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(new List<HistoricalDraw>(), groups);

        // Assert
        freq.Values.Sum().Should().Be(0);
    }

    [Test]
    public void Given_Single_Draw_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Count_All_WinningNumbers()
    {
        // Arrange
        var groups = G((1, 5), (6, 10));
        var history = new List<HistoricalDraw>
            {
                CreateDraw(new [] {1, 3, 7, 9}) // two in each group
            };

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(history, groups);

        // Assert
        (freq[(1, 5)] == 2 && freq[(6, 10)] == 2).Should().BeTrue();
    }

    [Test]
    public void Given_Multiple_Draws_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Accumulate_Counts()
    {
        // Arrange
        var groups = G((1, 5), (6, 10));
        var history = new List<HistoricalDraw>
            {
                CreateDraw([2, 6, 7]),
                CreateDraw([5, 8, 10]),
                CreateDraw([1, 9])
            };

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(history, groups);

        // Assert
        (freq[(1, 5)] == 3 && freq[(6, 10)] == 5).Should().BeTrue();
    }

    [Test]
    public void Given_Boundary_Values_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Treat_Bounds_As_Inclusive()
    {
        // Arrange
        var groups = G((1, 5), (6, 10));
        var history = new List<HistoricalDraw>
            {
                CreateDraw([1, 5, 6, 10])
            };

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(history, groups);

        // Assert
        (freq[(1, 5)] == 2 && freq[(6, 10)] == 2).Should().BeTrue();
    }

    [Test]
    public void Given_Values_Outside_All_Groups_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Ignore_Them()
    {
        // Arrange
        var groups = G((10, 12), (20, 22));
        var history = new List<HistoricalDraw>
            {
                CreateDraw([1, 9, 13, 19, 23]) // none in ranges
            };

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(history, groups);

        // Assert
        freq.Values.Sum().Should().Be(0);
    }

    [Test]
    public void Given_Overlapping_Groups_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Use_First_Matching_Group()
    {
        // Arrange: overlap at 5..6; FirstOrDefault should map to the earlier group
        var groups = G((1, 6), (5, 10));
        var history = new List<HistoricalDraw>
            {
                CreateDraw([5, 6])
            };

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(history, groups);

        // Assert
        (freq[(1, 6)] == 2 && freq[(5, 10)] == 0).Should().BeTrue();
    }

    [Test]
    public void Given_Duplicate_Numbers_In_A_Draw_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Count_Duplicates()
    {
        // Arrange (even if real draws are distinct, this guards counting logic)
        var groups = G((1, 5));
        var history = new List<HistoricalDraw>
            {
                CreateDraw([3, 3, 3])
            };

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(history, groups);

        // Assert
        freq[(1, 5)].Should().Be(3);
    }

    [Test]
    public void Given_Groups_With_No_Hits_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Keep_Zero_Entries()
    {
        // Arrange
        var groups = G((1, 5), (6, 10), (11, 15));
        var history = new List<HistoricalDraw>
            {
                CreateDraw([2, 3]) // hits only first group
            };

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(history, groups);

        // Assert
        (freq[(6, 10)] == 0 && freq[(11, 15)] == 0).Should().BeTrue();
    }

    private static HistoricalDraw CreateDraw(IEnumerable<int> winning)
    {
        return new HistoricalDraw(
            DrawId: 1,
            LotteryId: 999,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: winning.ToList(),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow
        );
    }

    private static List<(int start, int end)> G(params (int s, int e)[] xs)
        => xs.Select(x => (x.s, x.e)).ToList();
}