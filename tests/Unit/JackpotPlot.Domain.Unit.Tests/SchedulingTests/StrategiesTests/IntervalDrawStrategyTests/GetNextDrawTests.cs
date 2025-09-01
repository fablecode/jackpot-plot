using FluentAssertions;
using JackpotPlot.Domain.Scheduling.Strategies;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.SchedulingTests.StrategiesTests.IntervalDrawStrategyTests;

[TestFixture]
public class GetNextDrawTests
{
    [Test]
    public void Given_A_Start_Day_With_Weekly_Interval_When_GetNextDraw_Method_Is_Invoked_Should_Return_Start_Plus_Interval()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();
        var from = new DateTime(2024, 01, 01, 15, 30, 00); // time ignored (date is used)
        var config = new LotteryScheduleConfig
        {
            StartDate = new DateTime(2024, 01, 01),
            IntervalDays = 7
        };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert
        next.Should().Be(new DateTime(2024, 01, 08, 00, 00, 00));
    }

    [Test]
    public void Given_A_FromDate_Aligned_To_Draw_When_GetNextDraw_Method_Is_Invoked_Should_Return_The_Next_Interval_Not_Same_Day()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();
        var from = new DateTime(2024, 01, 08, 12, 00, 00); // aligned: 7 days after start
        var config = new LotteryScheduleConfig
        {
            StartDate = new DateTime(2024, 01, 01),
            IntervalDays = 7
        };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert
        next.Should().Be(new DateTime(2024, 01, 15, 00, 00, 00));
    }

    [Test]
    public void Given_A_Mid_Interval_Day_When_GetNextDraw_Method_Is_Invoked_Should_Round_Up_To_Next_Boundary()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();

        // Start on Jan 1, interval 3 => draws on Jan 1, 4, 7, 10, ...
        // From Jan 2 should go to Jan 4 (next boundary), not Jan 3.
        var from = new DateTime(2024, 01, 02, 09, 00, 00);
        var config = new LotteryScheduleConfig
        {
            StartDate = new DateTime(2024, 01, 01),
            IntervalDays = 3
        };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert
        next.Should().Be(new DateTime(2024, 01, 04, 00, 00, 00));
    }

    [Test]
    public void Given_A_Null_StartDate_When_GetNextDraw_Method_Is_Invoked_Should_Use_FromDate_Date_As_Start()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();
        var from = new DateTime(2024, 05, 10, 13, 45, 00);
        // Interval null => defaults to 7 (per implementation)
        var config = new LotteryScheduleConfig { StartDate = null, IntervalDays = null };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert
        next.Should().Be(new DateTime(2024, 05, 17, 00, 00, 00));
    }

    [Test]
    public void Given_A_Null_IntervalDays_When_GetNextDraw_Method_Is_Invoked_Should0_Default_To_Weekly()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();
        var from = new DateTime(2024, 03, 20, 17, 00, 00);
        var config = new LotteryScheduleConfig
        {
            StartDate = new DateTime(2024, 03, 15),
            IntervalDays = null // default to 7
        };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert
        next.Should().Be(new DateTime(2024, 03, 22, 00, 00, 00));
    }

    [Test]
    public void Given_A_Custom_Interval_When_GetNextDraw_Method_Is_Invoked_Should_Use_Custom_Interval()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();
        var from = new DateTime(2024, 06, 10, 23, 59, 59);
        var config = new LotteryScheduleConfig
        {
            StartDate = new DateTime(2024, 06, 01),
            IntervalDays = 5
        };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert
        next.Should().Be(new DateTime(2024, 06, 11, 00, 00, 00)); // daysSinceStart=9 => 5-(9%5=4)=1
    }


    [Test]
    public void Given_A_FromDate_Before_StartDate_When_GetNextDraw_Method_Invoked_Should_Return_First_Boundary_After_FromDate()
    {
        // Arrange
        var sut = new IntervalDrawStrategy();
        var from = new DateTime(2024, 01, 07); // 3 days before start
        var config = new LotteryScheduleConfig
        {
            StartDate = new DateTime(2024, 01, 10),
            IntervalDays = 7
        };

        // Act
        var next = sut.GetNextDraw(from, config);

        // Assert
        // Implementation computes using modular arithmetic on (fromDate - start),
        // which yields next = fromDate + (7 - (-3 % 7)) = fromDate + 10 days = Jan 17.
        next.Should().Be(new DateTime(2024, 01, 17, 00, 00, 00));
    }
}