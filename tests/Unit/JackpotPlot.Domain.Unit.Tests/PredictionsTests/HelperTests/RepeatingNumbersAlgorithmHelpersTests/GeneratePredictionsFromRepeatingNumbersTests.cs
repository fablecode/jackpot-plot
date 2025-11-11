using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.RepeatingNumbersAlgorithmHelpersTests;

[TestFixture]
public class GeneratePredictionsFromRepeatingNumbersTests
{
    [Test]
    public void Given_Repeating_Dictionary_When_GeneratePredictionsFromRepeatingNumbers_Is_Invoked_Should_Take_First_N_Keys_In_Input_Order()
    {
        // Arrange
        var repeating = new Dictionary<int, int> { { 7, 3 }, { 5, 2 }, { 9, 2 } };

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.GeneratePredictionsFromRepeatingNumbers(repeating, count: 2);

        // Assert
        result.Should().Equal(new[] { 7, 5 });
    }

    [Test]
    public void Given_Count_Exceeding_Keys_When_GeneratePredictionsFromRepeatingNumbers_Is_Invoked_Should_Return_All_Keys()
    {
        // Arrange
        var repeating = new Dictionary<int, int> { { 4, 2 }, { 1, 2 } };

        // Act
        var result = RepeatingNumbersAlgorithmHelpers.GeneratePredictionsFromRepeatingNumbers(repeating, count: 5);

        // Assert
        result.Should().Equal(new[] { 4, 1 });
    }
}