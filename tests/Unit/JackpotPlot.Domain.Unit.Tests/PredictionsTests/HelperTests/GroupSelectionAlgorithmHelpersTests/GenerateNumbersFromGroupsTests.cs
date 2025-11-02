using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.GroupSelectionAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersFromGroupsTests
{
    [Test]
    public void Given_Non_Positive_TotalCount_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(40, 4);
        var freq = groups.ToDictionary(g => g, _ => 0);
        var rng = new Random(1);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 0, rng);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Given_Zero_History_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Return_Configured_Count()
    {
        // Arrange
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(40, 4);
        var freq = groups.ToDictionary(g => g, _ => 0); // triggers equal split path
        var rng = new Random(2);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 7, rng);

        // Assert
        result.Length.Should().Be(7);
    }

    [Test]
    public void Given_Groups_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Keep_Numbers_In_Global_Range()
    {
        // Arrange
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(40, 4);
        var freq = groups.ToDictionary(g => g, _ => 0);
        var rng = new Random(3);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 10, rng);

        // Assert
        result.All(n => n >= groups.First().start && n <= groups.Last().end).Should().BeTrue();
    }

    [Test]
    public void Given_Groups_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Not_Contain_Duplicates()
    {
        // Arrange
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(20, 4);
        var freq = groups.ToDictionary(g => g, _ => 0);
        var rng = new Random(4);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 12, rng);

        // Assert
        result.Distinct().Count().Should().Be(result.Length);
    }

    [Test]
    public void Given_Same_Seed_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Be_Deterministic()
    {
        // Arrange
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(30, 3);
        var freq = groups.ToDictionary(g => g, _ => 0);
        var rng1 = new Random(42);
        var rng2 = new Random(42);

        // Act
        var a = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 9, rng1);
        var b = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 9, rng2);

        // Assert
        a.Should().Equal(b);
    }

    [Test]
    public void Given_Skewed_Frequencies_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Allocate_Proportionally_After_Normalization()
    {
        // Arrange: two groups [1..10] and [11..20], heavy weight on first
        var groups = new List<(int start, int end)> { (1, 10), (11, 20) };
        var freq = new Dictionary<(int, int), int> { [(1, 10)] = 9, [(11, 20)] = 1 };
        var rng = new Random(123);

        // Act: totalCount 5 → initial shares 4.5 & 0.5 rounded away from zero ⇒ 5 & 1, then normalized to 4 & 1
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 5, rng);
        var perGroup = CountPerGroup(result, groups);

        // Assert
        (perGroup[(1, 10)] == 4 && perGroup[(11, 20)] == 1).Should().BeTrue();
    }

    [Test]
    public void Given_Sum_Rounds_Below_Total_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Boost_Largest_First_To_Exact_Total()
    {
        // Arrange: three groups equal freq, totalCount=1 → initial 0,0,0 then normalization adds 1 to first
        var groups = new List<(int start, int end)> { (1, 10), (11, 20), (21, 30) };
        var freq = groups.ToDictionary(g => g, _ => 1);
        var rng = new Random(42);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 1, rng);
        var perGroup = CountPerGroup(result, groups);

        // Assert
        perGroup[(1, 10)].Should().Be(1);
    }

    [Test]
    public void Given_Rounded_Sum_Exceeds_Total_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Shrink_Largest_First_To_Exact_Total()
    {
        // Arrange: three groups equal freq, totalCount=2 → initial 1,1,1 (sum 3) then normalization → 1,1,0
        var groups = new List<(int start, int end)> { (1, 10), (11, 20), (21, 30) };
        var freq = groups.ToDictionary(g => g, _ => 1);
        var rng = new Random(7);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 2, rng);
        var perGroup = CountPerGroup(result, groups);

        // Assert
        perGroup.Values.OrderBy(v => v).SequenceEqual([0, 1, 1]).Should().BeTrue();
    }

    [Test]
    public void Given_Overlapping_Groups_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Avoid_Duplicates_Across_Groups()
    {
        // Arrange: overlapping at 3 (first 1–3, second 3–5); proportional split forces both groups to pick
        var groups = new List<(int start, int end)> { (1, 3), (3, 5) };
        var freq = new Dictionary<(int, int), int> { [(1, 3)] = 5, [(3, 5)] = 5 };
        var rng = new Random(9);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 5, rng);

        // Assert
        result.Distinct().Count().Should().Be(result.Length);
    }

    [Test]
    public void Given_TotalCount_Exceeds_Global_Range_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Return_As_Many_As_Possible()
    {
        // Arrange: only three unique numbers exist globally, ask for five
        var groups = new List<(int start, int end)> { (1, 1), (2, 2), (3, 3) };
        var freq = groups.ToDictionary(g => g, _ => 10);
        var rng = new Random(10);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 5, rng);

        // Assert
        result.Length.Should().Be(3);
    }

    [Test]
    public void Given_Equal_Frequencies_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Respect_Proportionality_After_Rounding()
    {
        // Arrange: four groups equal freq, pick 7 → initial proportional 1.75 each ⇒ rounds to 2,2,2,2 (8) then normalized to 2,2,2,1
        var groups = new List<(int start, int end)> { (1, 5), (6, 10), (11, 15), (16, 20) };
        var freq = groups.ToDictionary(g => g, _ => 1);
        var rng = new Random(99);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 7, rng);
        var perGroup = CountPerGroup(result, groups);

        // Assert
        perGroup.Values.OrderBy(v => v).SequenceEqual([1, 2, 2, 2]).Should().BeTrue();
    }

    [Test]
    public void Given_Partial_Selections_When_GenerateNumbersFromGroups_Method_Is_Invoked_Should_Fill_Remaining_Uniformly_Across_Global_Range()
    {
        // Arrange: tight groups make it easy to run short in per-group sampling, then uniform fill kicks in
        var groups = new List<(int start, int end)> { (1, 2), (3, 4) };
        var freq = new Dictionary<(int, int), int> { [(1, 2)] = 1, [(3, 4)] = 1 };
        var rng = new Random(777);

        // Act
        var result = GroupSelectionAlgorithmHelpers.GenerateNumbersFromGroups(groups, freq, 3, rng);

        // Assert
        result.Length.Should().Be(3);
    }

    private static Dictionary<(int start, int end), int> CountPerGroup(IEnumerable<int> numbers, List<(int start, int end)> groups)
    {
        var dict = groups.ToDictionary(g => g, _ => 0);
        foreach (var n in numbers)
        {
            var g = groups.FirstOrDefault(x => n >= x.start && n <= x.end);
            if (g != default) dict[g]++;
        }
        return dict;
    }
}