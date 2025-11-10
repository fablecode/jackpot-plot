using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.PredictionsTests.HelperTests.NumberChainAlgorithmHelpersTests;

[TestFixture]
public class GenerateNumbersFromChainsTests
{
    [Test]
    public void Given_Enough_In_First_Chain_When_GenerateNumbersFromChains_Method_Is_Invoked_Should_Select_From_First_Chain()
    {
        // Arrange
        var numberChains = new Dictionary<HashSet<int>, int>(HashSet<int>.CreateSetComparer())
        {
            [[7, 8, 9]] = 5,
            [[1, 2, 3]] = 4
        };
        var rng = new Random(123);

        // Act
        var result = NumberChainAlgorithmHelpers.GenerateNumbersFromChains(numberChains, count: 2, rng);

        // Assert
        result.ToHashSet().IsSubsetOf([7, 8, 9]).Should().BeTrue();
    }

    [Test]
    public void Given_Multiple_Chains_When_GenerateNumbersFromChains_Method_Is_Invoked_Should_Return_Requested_Count()
    {
        // Arrange
        var numberChains = new Dictionary<HashSet<int>, int>(HashSet<int>.CreateSetComparer())
        {
            [[4]] = 10,
            [[1, 2, 3]] = 9
        };
        var rng = new Random(456);

        // Act
        var result = NumberChainAlgorithmHelpers.GenerateNumbersFromChains(numberChains, count: 3, rng);

        // Assert
        result.Count.Should().Be(3);
    }
}