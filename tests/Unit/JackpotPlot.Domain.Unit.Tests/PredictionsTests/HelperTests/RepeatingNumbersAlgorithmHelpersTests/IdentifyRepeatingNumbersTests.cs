using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.RepeatingNumbersAlgorithmHelpersTests;

[TestFixture]
public class IdentifyRepeatingNumbersTests
{
    [Test]
    public void Given_Numbers_With_Repetitions_When_IdentifyRepeatingNumbers_Is_Invoked_Should_Return_Only_Repeated_With_Counts()
    {
        // Arrange
        var numbers = new List<int> { 1, 2, 2, 3, 3, 3, 4 };

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.IdentifyRepeatingNumbers(numbers);

        // Assert
        result.Should().BeEquivalentTo(new Dictionary<int, int> { { 3, 3 }, { 2, 2 } });
    }

    [Test]
    public void Given_All_Unique_Numbers_When_IdentifyRepeatingNumbers_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var numbers = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.IdentifyRepeatingNumbers(numbers);

        // Assert
        result.Should().BeEmpty();
    }
}