using FluentAssertions;
using JackpotPlot.Domain.Scheduling.Strategies;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.SchedulingTests.StrategiesTests.DailyDrawStrategyTests;

[TestFixture]
public class GetNextDrawTests
{
    // Note: LotteryScheduleConfig is not used by DailyDrawStrategy.
    // If you have a concrete type, replace `default!` with `new LotteryScheduleConfig(...)`.

    [Test]
    public void Given_FromDate_With_Time_When_GetNextDraw_Method_Is_Invoked_Should_Return_Next_Day_At_Midnight()
    {
        // Arrange
        var sut = new DailyDrawStrategy();
        var from = new DateTime(2024, 05, 10, 13, 45, 22);

        // Act
        var next = sut.GetNextDraw(from, config: null!);

        // Assert
        next.Should().Be(new DateTime(2024, 05, 11, 00, 00, 00));
    }

    [Test]
    public void Given_End_Of_Month_Date_With_Time_When_GetNextDraw_Method_Is_Invoked_Should_Roll_To_First_Of_Next_Month_At_Midnight()
    {
        // Arrange
        var sut = new DailyDrawStrategy();
        var from = new DateTime(2024, 01, 31, 23, 59, 59);

        // Act
        var next = sut.GetNextDraw(from, config: null!);

        // Assert
        next.Should().Be(new DateTime(2024, 02, 01, 00, 00, 00));
    }

    [Test]
    public void Given_End_Of_Year_Date_With_Time_When_GetNextDraw_Method_Is_Invoked_Should_Roll_To_Jan_1_At_Midnight()
    {
        // Arrange
        var sut = new DailyDrawStrategy();
        var from = new DateTime(2024, 12, 31, 10, 00, 00);

        // Act
        var next = sut.GetNextDraw(from, config: null!);

        // Assert
        next.Should().Be(new DateTime(2025, 01, 01, 00, 00, 00));
    }

    [Test]
    public void Given_Leap_Year_Feb_28_When_GetNextDraw_Method_Is_Invoked_Should_Return_Feb_29_At_Midnight()
    {
        // Arrange
        var sut = new DailyDrawStrategy();
        var from = new DateTime(2024, 02, 28, 08, 00, 00); // 2024 is a leap year

        // Act
        var next = sut.GetNextDraw(from, config: null!);

        // Assert
        next.Should().Be(new DateTime(2024, 02, 29, 00, 00, 00));
    }

    [Test]
    public void Given_Leap_Day_When_GetNextDraw_Is_Invoked_Should_Return_March_1_At_Midnight()
    {
        // Arrange
        var sut = new DailyDrawStrategy();
        var from = new DateTime(2024, 02, 29, 18, 30, 00); // Leap day

        // Act
        var next = sut.GetNextDraw(from, config: null!);

        // Assert
        next.Should().Be(new DateTime(2024, 03, 01, 00, 00, 00));
    }
}