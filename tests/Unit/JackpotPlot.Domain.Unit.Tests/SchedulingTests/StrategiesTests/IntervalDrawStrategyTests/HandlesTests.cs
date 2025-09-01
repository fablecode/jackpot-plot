using FluentAssertions;
using JackpotPlot.Domain.Scheduling.Strategies;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.SchedulingTests.StrategiesTests.IntervalDrawStrategyTests;

[TestFixture]
public class HandlesTests
{
    [Test]
    public void Given_An_Interval_ScheduleType_When_Handles_Method_Invoked_Should_Return_True()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();

        // Act
        var handles = sut.Handles(DrawScheduleType.Interval);

        // Assert
        handles.Should().BeTrue();
    }

    [Test]
    public void Given_A_NonInterval_ScheduleType_When_Handles_Method_Is_Invoked_Should_Return_False()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();

        // Act
        var handles = sut.Handles(DrawScheduleType.Daily); // or DrawScheduleType.Weekday

        // Assert
        handles.Should().BeFalse();
    }
}