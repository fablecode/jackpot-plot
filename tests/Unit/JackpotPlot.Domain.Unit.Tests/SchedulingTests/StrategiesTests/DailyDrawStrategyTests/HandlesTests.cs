using FluentAssertions;
using JackpotPlot.Domain.Scheduling.Strategies;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.SchedulingTests.StrategiesTests.DailyDrawStrategyTests;

[TestFixture]
public class HandlesTests
{
    [Test]
    public void Given_A_Valid_Daily_ScheduleType_When_Handles_Method_Is_Invoked_Should_Return_True()
    {
        // Arrange
        var sut = new DailyDrawStrategy();

        // Act
        var handles = sut.Handles(DrawScheduleType.Daily);

        // Assert (one)
        handles.Should().BeTrue();
    }

    [Test]
    public void Given_An_Invalid_Daily_ScheduleType_When_Handles_Method_Is_Invoked_Should_Return_False()
    {
        // Arrange
        var sut = new DailyDrawStrategy();

        // Act
        var handles = sut.Handles((DrawScheduleType)999); // any non-Daily value

        // Assert (one)
        handles.Should().BeFalse();
    }
}