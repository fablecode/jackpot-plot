using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.ConsecutiveNumbersAlgorithmHelpersTests;

[TestFixture]
public class GenerateRandomNumbersTests
{
    [Test]
    public void Given_Range_And_Count_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var rng = new Random(123);

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.GenerateRandomNumbers(1, 10, [], 4, rng);

        // Assert
        result.Length.Should().Be(4);
    }

    [Test]
    public void Given_Exclusions_When_GenerateRandomNumbers_Method_Is_Invoked_Result_Should_Not_Contain_Excluded()
    {
        // Arrange
        var rng = new Random(123);
        var exclude = new List<int> { 3, 4, 5 };

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.GenerateRandomNumbers(1, 10, exclude, 6, rng);

        // Assert
        result.Should().NotContain(exclude);
    }

    [Test]
    public void Given_Range_When_GenerateRandomNumbers_Method_Is_Invoked_Should_Return_Numbers_Within_Range()
    {
        // Arrange
        var rng = new Random(123);

        // Act
        var result = ConsecutiveNumbersAlgorithmHelpers.GenerateRandomNumbers(5, 7, [], 2, rng);

        // Assert
        result.All(n => n >= 5 && n <= 7).Should().BeTrue();
    }
}