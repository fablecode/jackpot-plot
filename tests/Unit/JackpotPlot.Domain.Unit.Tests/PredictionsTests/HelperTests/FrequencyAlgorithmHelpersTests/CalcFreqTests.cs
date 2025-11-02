using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.FrequencyAlgorithmHelpersTests;

[TestFixture]
public class CalcFreqTests
{
    [Test]
    public void Given_Range_When_CalcFreq_Method_Is_Invoked_List_Length_Should_Equal_Range()
    {
        // Arrange
        var numbers = new List<int> { 1, 2, 2, 5 };

        // Act
        var result = FrequencyAlgorithmHelpers.CalcFreq(numbers, range: 10);

        // Assert
        result.Count.Should().Be(10);
    }

    [Test]
    public void Given_Numbers_When_CalcFreq_Method_Is_Invoked_Should_Count_Frequency_For_Specific_Number()
    {
        // Arrange
        var numbers = new List<int> { 1, 1, 2, 5 };

        // Act
        var result = FrequencyAlgorithmHelpers.CalcFreq(numbers, range: 5);

        // Assert
        result.Single(x => x.Number == 1).Frequency.Should().Be(2);
    }

    [Test]
    public void Given_Empty_List_When_CalcFreq_Method_Is_Invoked_Should_Return_All_Zeros()
    {
        // Arrange
        var numbers = new List<int>();

        // Act
        var result = FrequencyAlgorithmHelpers.CalcFreq(numbers, range: 7);

        // Assert
        result.Sum(x => x.Frequency).Should().Be(0);
    }
}