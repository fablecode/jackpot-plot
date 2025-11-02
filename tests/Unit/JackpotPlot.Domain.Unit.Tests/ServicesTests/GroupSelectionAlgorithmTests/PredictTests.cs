using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.GroupSelectionAlgorithmTests;

[TestFixture]
public class PredictTests
{
    [Test]
    public void Given_Range_And_GroupCount_When_DivideIntoGroups_Method_Is_Invoked_Should_Partition_Entire_Range()
    {
        // Arrange
        const int range = 17;
        const int groups = 4;

        // Act
        var result = GroupSelectionAlgorithmHelpers.DivideIntoGroups(range, groups);

        // Assert
        result.Last().end.Should().Be(range);
    }

    [Test]
    public void Given_Remainder_When_DivideIntoGroups_Method_Is_Invoked_Should_Distribute_Extras_To_Earlier_Groups()
    {
        // Arrange
        // baseSize = 10/3 = 3, remainder = 1 → first group gets +1 → size = 4
        const int range = 10;
        const int groups = 3;

        // Act
        var result = GroupSelectionAlgorithmHelpers.DivideIntoGroups(range, groups);

        // Assert
        (result[0].end - result[0].start + 1).Should().Be(4);
    }

    [Test]
    public void Given_Draws_When_AnalyzeGroupFrequencies_Method_Is_Invoked_Should_Count_Numbers_Per_Group()
    {
        // Arrange
        var groups = new List<(int start, int end)> { (1, 5), (6, 10) };
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 6, 10) };

        // Act
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(draws, groups);

        // Assert
        freq[(1, 5)].Should().Be(2);
    }

    [Test]
    public void Given_Non_Positive_Target_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var groups = new List<(int start, int end)> { (1, 5), (6, 10) };
        var freq = groups.ToDictionary(g => g, _ => 0);
        var rng = new Random(1);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_No_History_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Allocate_Proportionally_And_Return_TotalCount()
    {
        // Arrange
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(12, 3); // (1..4),(5..8),(9..12)
        var freq = groups.ToDictionary(g => g, _ => 0);
        var rng = new Random(2);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, totalCount: 5, rng);

        // Assert
        result.Length.Should().Be(5);
    }

    [Test]
    public void Given_History_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Return_Distinct_In_Range()
    {
        // Arrange
        var groups = new List<(int start, int end)> { (1, 3), (4, 6), (7, 9) };
        var draws = new[] { AlgorithmsTestHelperTests.Draw(1, 2, 5, 5, 8, 9) };
        var freq = GroupSelectionAlgorithmHelpers.AnalyzeGroupFrequencies(draws, groups);
        var rng = new Random(3);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, totalCount: 6, rng);

        // Assert
        (result.Distinct().Count() == result.Length &&
         result.All(n => n >= 1 && n <= 9)).Should().BeTrue();
    }

    [Test]
    public void Given_Tight_Group_Ranges_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Fill_Shortfall_To_Reached_Total()
    {
        // Arrange
        var groups = new List<(int start, int end)> { (1, 1), (2, 2) }; // only 2 unique possible
        var freq = groups.ToDictionary(g => g, _ => 10);
        var rng = new Random(4);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, totalCount: 4, rng);

        // Assert
        result.Length.Should().Be(2);
    }

    [Test]
    public void Given_Empty_Predicted_When_CalculateGroupConfidence_Is_Invoked_Should_Should_Return_Zero()
    {
        // Arrange
        var groups = new List<(int start, int end)> { (1, 5), (6, 10) };
        var histCounts = groups.ToDictionary(g => g, _ => 0);

        // Act
        var confidence = GroupSelectionAlgorithmHelpers.CalculateGroupConfidence(Array.Empty<HistoricalDraw>(), ImmutableArray<int>.Empty, groups, histCounts);

        // Assert
        confidence.Should().Be(0.0);
    }

    [Test]
    public void Given_Identical_Distributions_When_CalculateGroupConfidence_Is_Invoked_Should_Should_Return_One()
    {
        // Arrange
        var groups = new List<(int start, int end)> { (1, 5), (6, 10) };
        var predicted = ImmutableArray.Create(1, 3, 6, 8); // two in each group
        var histCounts = new Dictionary<(int start, int end), int>
        {
            [(1, 5)] = 2,
            [(6, 10)] = 2
        };

        // Act
        var confidence = GroupSelectionAlgorithmHelpers.CalculateGroupConfidence(Array.Empty<HistoricalDraw>(), predicted, groups, histCounts);

        // Assert
        confidence.Should().Be(1.0);
    }

    // ========== RandomDistinct ==========

    [Test]
    public void Given_Non_Positive_Count_When_RandomDistinct_Is_Invoked_Should_Should_Return_Empty()
    {
        // Arrange
        var rng = new Random(5);

        // Act
        var result = GroupSelectionAlgorithmHelpers.RandomDistinct(1, 10, ImmutableArray<int>.Empty, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Exclusions_When_RandomDistinct_Is_Invoked_Should_Should_Not_Return_Excluded()
    {
        // Arrange
        var rng = new Random(6);
        var exclude = ImmutableArray.Create(3, 4, 5);

        // Act
        var result = GroupSelectionAlgorithmHelpers.RandomDistinct(1, 10, exclude, 5, rng);

        // Assert
        result.Intersect(exclude).Any().Should().BeFalse();
    }

    [Test]
    public void Given_Range_When_RandomDistinct_Is_Invoked_Should_Should_Return_In_Range_And_Distinct()
    {
        // Arrange
        var rng = new Random(7);

        // Act
        var result = GroupSelectionAlgorithmHelpers.RandomDistinct(5, 7, ImmutableArray<int>.Empty, 2, rng);

        // Assert
        (result.All(n => n >= 5 && n <= 7) && result.Distinct().Count() == result.Length).Should().BeTrue();
    }
}