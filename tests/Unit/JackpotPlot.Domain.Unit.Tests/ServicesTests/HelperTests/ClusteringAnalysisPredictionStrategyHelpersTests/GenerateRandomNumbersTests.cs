using NUnit.Framework;
using FluentAssertions;
using JackpotPlot.Domain.Services.PredictionStrategies.Helpers;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.ClusteringAnalysisPredictionStrategyHelpersTests;

[TestFixture]
public class GenerateRandomNumbersTests
{
    [Test]
    public void Given_A_Valid_Range_And_Exclusions_When_GenerateRandomNumbers_Method_Is_Invoked_Then_It_Returns_Correct_Count()
    {
        // Arrange
        const int min = 1;
        const int max = 10;
        var exclude = new List<int> { 2, 3 };
        const int count = 5;
        var random = new Random(1234); // seeded for determinism

        // Act
        var result = ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(min, max, exclude, count, random);

        // Assert
        result.Length.Should().Be(count);
    }

    [Test]
    public void Given_A_Valid_Range_And_Exclusions_When_GenerateRandomNumbers_Method_Is_Invoked_Then_It_Returns_Valid_Numbers()
    {
        // Arrange
        const int min = 1;
        const int max = 10;
        var exclude = new List<int> { 2, 3 };
        const int count = 5;
        var random = new Random(1234); // seeded for determinism

        // Act
        var result = ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(min, max, exclude, count, random);

        // Assert
        result.Should().OnlyContain(num => num >= min && num <= max && !exclude.Contains(num));
    }

    [Test]
    public void Given_A_Valid_Range_And_Exclusions_When_GenerateRandomNumbers_Method_Is_Invoked_Result_Should_Only_Have_Unique_Items()
    {
        // Arrange
        const int min = 1;
        const int max = 10;
        var exclude = new List<int> { 2, 3 };
        const int count = 5;
        var random = new Random(1234); // seeded for determinism

        // Act
        var result = ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(min, max, exclude, count, random);

        // Assert
        result.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void Given_A_Zero_Count_When_GenerateRandomNumbers_Method_Invoked_Then_It_Returns_An_Empty_ImmutableArray()
    {
        // Arrange
        const int min = 1;
        const int max = 10;
        var exclude = new List<int>(); // no numbers to exclude
        const int count = 0;
        var random = new Random(1234);

        // Act
        var result = ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(min, max, exclude, count, random);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_A_Count_GreaterThan_The_Available_Numbers_When_GenerateRandomNumbers_Method_Is_Invoked_Then_It_Returns_A_List_With_Two_Available_Numbers()
    {
        // Arrange
        const int min = 1;
        const int max = 5;
        // Exclude most numbers so that only two remain.
        var exclude = new List<int> { 3, 4, 5 }; // availableNumbers will be {1, 2}
        const int count = 5; // request more than available
        var random = new Random(1234);

        // Act
        var result = ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(min, max, exclude, count, random);

        // Assert
        result.Length.Should().Be(2);
    }

    [Test]
    public void Given_A_Count_GreaterThan_The_Available_Numbers_When_GenerateRandomNumbers_Method_Is_Invoked_Then_It_Returns_All_Available_Numbers()
    {
        // Arrange
        const int min = 1;
        const int max = 5;
        // Exclude most numbers so that only two remain.
        var exclude = new List<int> { 3, 4, 5 }; // availableNumbers will be {1, 2}
        const int count = 5; // request more than available
        var random = new Random(1234);

        // Act
        var result = ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(min, max, exclude, count, random);

        // Assert
        result.Should().BeEquivalentTo(new List<int> { 1, 2 });
    }

    [Test]
    public void Given_Same_Seed_When_GenerateRandomNumbers_Method_Is_Invoked_Then_It_Returns_ConsistentResults()
    {
        // Arrange
        const int min = 1;
        const int max = 10;
        var exclude = new List<int> { 2, 3 };
        const int count = 5;
        var random1 = new Random(1234);
        var random2 = new Random(1234);

        // Act
        var result1 = ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(min, max, exclude, count, random1);
        var result2 = ClusteringAnalysisPredictionStrategyHelpers.GenerateRandomNumbers(min, max, exclude, count, random2);

        // Assert
        result1.Should().Equal(result2);
    }
}