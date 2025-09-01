using FluentAssertions;
using JackpotPlot.Domain.Scheduling.Strategies;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.SchedulingTests.StrategiesTests.WeekdayDrawStrategyTests;

[TestFixture]
public class HandlesTests
{
    [Test]
    public void Given_Weekday_ScheduleType_When_Handles_Method_Is_Invoked_Should_Return_True()
    {
        // Arrange
        var sut = new WeekdayDrawStrategy();

        // Act
        var handles = sut.Handles(DrawScheduleType.Weekday);

        // Assert (one)
        handles.Should().BeTrue();
    }

    [Test]
    public void Given_Non_Weekday_ScheduleType_When_Handles_Method_Is_Invoked_Should_Return_False()
    {
        // Arrange
        var sut = new WeekdayDrawStrategy();

        // Act
        var handles = sut.Handles(DrawScheduleType.Daily);

        // Assert (one)
        handles.Should().BeFalse();
    }
}