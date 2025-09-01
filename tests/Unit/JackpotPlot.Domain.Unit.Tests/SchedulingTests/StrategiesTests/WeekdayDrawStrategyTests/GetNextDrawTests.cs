using FluentAssertions;
using JackpotPlot.Domain.Scheduling.Strategies;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.SchedulingTests.StrategiesTests.WeekdayDrawStrategyTests;

[TestFixture]
public class GetNextDrawTests
{
    [Test]
    public void Given_Null_Days_When_GetNextDraw_Method_Is_Invoked_Should_Throw_ArgumentException()
    {
        // Arrange
        var sut = new WeekdayDrawStrategy();
        var from = new DateTime(2024, 05, 10, 13, 45, 00);
        var config = new LotteryScheduleConfig { Days = null };

        // Act
        var act = () => sut.GetNextDraw(from, config);

        // Assert (one)
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Given_Empty_Days_When_GetNextDraw_Method_Is_Invoked_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var sut = new WeekdayDrawStrategy();
        var from = new DateTime(2024, 05, 10);
        var config = new LotteryScheduleConfig { Days = new List<DayOfWeek>() };

        // Act
        var act = () => sut.GetNextDraw(from, config);

        // Assert (one)
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Given_FromDate_On_Allowed_Weekday_When_GetNextDraw_Method_Is_Invoked_Should_Return_Same_Day_At_Midnight()
    {
        // Arrange
        var sut = new WeekdayDrawStrategy();
        var from = new DateTime(2024, 05, 06, 17, 30, 00); // Monday
        var config = new LotteryScheduleConfig { Days = [DayOfWeek.Monday] };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert (one)
        next.Should().Be(new DateTime(2024, 05, 06, 00, 00, 00));
    }

    [Test]
    public void Given_FromDate_Not_On_Allowed_Weekday_When_GetNextDraw_Method_Is_Invoked_Should_Return_Next_Allowed_Day()
    {
        // Arrange
        var sut = new WeekdayDrawStrategy();
        var from = new DateTime(2024, 05, 07, 10, 00, 00); // Tuesday
        var config = new LotteryScheduleConfig { Days = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Thursday } };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert (one)
        next.Should().Be(new DateTime(2024, 05, 09, 00, 00, 00)); // Thursday
    }

    [Test]
    public void Given_Date_Near_Week_End_When_GetNextDraw_Method_Is_Invoked_Should_Roll_Into_Next_Week()
    {
        // Arrange
        var sut = new WeekdayDrawStrategy();
        var from = new DateTime(2024, 05, 07); // Tuesday
        var config = new LotteryScheduleConfig { Days = new List<DayOfWeek> { DayOfWeek.Monday } };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert (one)
        next.Should().Be(new DateTime(2024, 05, 13, 00, 00, 00)); // next Monday
    }

    [Test]
    public void Given_Time_Component_When_GetNextDraw_Method_Is_Invoked_Then_Time_Is_Normalized_To_Midnight()
    {
        // Arrange
        var sut = new WeekdayDrawStrategy();
        var from = new DateTime(2024, 05, 08, 23, 59, 59); // Wednesday
        var config = new LotteryScheduleConfig { Days = new List<DayOfWeek> { DayOfWeek.Wednesday } };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert (one)
        next.Should().Be(new DateTime(2024, 05, 08, 00, 00, 00));
    }

}