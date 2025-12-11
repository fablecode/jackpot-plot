using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Validations;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.ValidationsTests;

[TestFixture]
public class LotteryDrawnEventMessageValidationTests
{
    private LotteryDrawnEventMessageValidation _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new LotteryDrawnEventMessageValidation();
    }

    [Test]
    public void Given_Valid_Message_When_Validate_Is_Invoked_Should_Return_Valid()
    {
        // Arrange
        var message = CreateMessage(
            eventType: EventTypes.LotteryDrawn,
            lotteryId: 1);

        // Act
        var result = _sut.Validate(message);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_Wrong_EventType_When_Validate_Is_Invoked_Should_Return_Invalid()
    {
        // Arrange
        var message = CreateMessage(
            eventType: EventTypes.EurojackpotDraw,
            lotteryId: 1);

        // Act
        var result = _sut.Validate(message);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void Given_Wrong_EventType_When_Validate_Is_Invoked_Should_Have_Error_On_Event()
    {
        // Arrange
        var message = CreateMessage(
            eventType: "some-other-event",
            lotteryId: 1);

        // Act
        var result = _sut.Validate(message);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == "Event");
    }

    [Test]
    public void Given_LotteryId_Less_Than_Or_Equal_To_Zero_When_Validate_Is_Invoked_Should_Return_Invalid()
    {
        // Arrange
        var message = CreateMessage(
            eventType: EventTypes.LotteryDrawn,
            lotteryId: 0);

        // Act
        var result = _sut.Validate(message);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void Given_LotteryId_Less_Than_Or_Equal_To_Zero_When_Validate_Is_Invoked_Should_Have_Error_On_Data_LotteryId()
    {
        // Arrange
        var message = CreateMessage(
            eventType: EventTypes.LotteryDrawn,
            lotteryId: -5);

        // Act
        var result = _sut.Validate(message);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == "Data.LotteryId");
    }

    private static Message<LotteryDrawnEvent> CreateMessage(
        string eventType,
        int lotteryId)
    {
        var data = new LotteryDrawnEvent
        {
            LotteryId = lotteryId,
            DrawDate = DateTime.UtcNow,
            WinningNumbers = ImmutableArray<int>.Empty,
            BonusNumbers = ImmutableArray<int>.Empty
        };

        return new Message<LotteryDrawnEvent>(eventType, data);
    }
}