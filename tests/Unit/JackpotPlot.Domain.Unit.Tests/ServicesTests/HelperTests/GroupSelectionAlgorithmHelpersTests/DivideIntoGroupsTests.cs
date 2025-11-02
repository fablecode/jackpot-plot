using FluentAssertions;
using JackpotPlot.Domain.Predictions.Helpers;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.ServicesTests.HelperTests.GroupSelectionAlgorithmHelpersTests;

[TestFixture]
public class DivideIntoGroupsTests
{
    [Test]
    public void Given_Even_Division_When_DivideIntoGroups_Method_Is_Invoked_Should_Return_Expected_Group_Count()
    {
        // Arrange
        const int numberRange = 50;
        const int groupCount = 5;

        // Act
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(numberRange, groupCount);

        // Assert
        groups.Count.Should().Be(5);
    }

    [Test]
    public void Given_Remainder_When_DivideIntoGroups_Method_Is_Invoked_Should_Distribute_Extra_On_Earlier_Groups()
    {
        // Arrange
        const int numberRange = 10;
        const int groupCount = 3;

        // Act
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(numberRange, groupCount);

        // Assert (sizes should be 4,3,3)
        groups.Select(g => g.end - g.start + 1).ToArray().Should().BeEquivalentTo(new[] { 4, 3, 3 });
    }

    [Test]
    public void Given_Range_When_DivideIntoGroups_Method_Is_Invoked_Should_Cover_Exactly_The_Range()
    {
        // Arrange
        const int numberRange = 37;
        const int groupCount = 4;

        // Act
        var groups = GroupSelectionAlgorithmHelpers.DivideIntoGroups(numberRange, groupCount);

        // Assert
        (groups.First().start == 1 && groups.Last().end == numberRange).Should().BeTrue();
    }
}