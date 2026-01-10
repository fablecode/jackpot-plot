using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Features.AddTicketPlays;
using JackpotPlot.Lottery.API.Application.Models.Input;
using Lottery.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;
using MediatR;

namespace Lottery.API.Unit.Tests.ControllersTests.PlaysControllerTests;

[TestFixture]
public class PostTests
{
    private IMediator _mediator;
    private PlaysController _sut;

    [SetUp]
    public void SetUp()
    {
        _mediator = Substitute.For<IMediator>();
        _sut = new PlaysController(_mediator);
    }

    [Test]
    public async Task Given_Success_Result_When_Post_Is_Invoked_Should_Return_CreatedAtAction()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[] { new CreateTicketPlaysInput(LineIndex: 1, Numbers: [1, 2, 3, 4, 5]) };
        var createdIds = ImmutableArray.Create(Guid.NewGuid());

        _mediator
            .Send(Arg.Any<AddTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<Guid>>.Success(createdIds));

        // Act
        var result = await _sut.Post(ticketId, plays);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_Post_Is_Invoked_Should_Return_Created_Value_As_Result_Value()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[] { new CreateTicketPlaysInput(LineIndex: 1, Numbers: [1, 2, 3, 4, 5]) };
        var createdIds = ImmutableArray.Create(Guid.NewGuid());

        _mediator
            .Send(Arg.Any<AddTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<Guid>>.Success(createdIds));

        // Act
        var result = await _sut.Post(ticketId, plays);

        // Assert
        ((CreatedAtActionResult)result).Value.Should().Be(createdIds);
    }

    [Test]
    public async Task Given_Success_Result_When_Post_Is_Invoked_Should_Set_CreatedAtAction_Name_To_Get()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[] { new CreateTicketPlaysInput(LineIndex: 1, Numbers: [1, 2, 3, 4, 5]) };
        var createdIds = ImmutableArray.Create(Guid.NewGuid());

        _mediator
            .Send(Arg.Any<AddTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<Guid>>.Success(createdIds));

        // Act
        var result = await _sut.Post(ticketId, plays);

        // Assert
        ((CreatedAtActionResult)result).ActionName.Should().Be(nameof(PlaysController.Get));
    }

    [Test]
    public async Task Given_Failure_Result_When_Post_Is_Invoked_Should_Return_BadRequest()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[] { new CreateTicketPlaysInput(LineIndex: 1, Numbers: [1, 2, 3, 4, 5]) };

        _mediator
            .Send(Arg.Any<AddTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<Guid>>.Failure("failed"));

        // Act
        var result = await _sut.Post(ticketId, plays);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Given_TicketId_And_Plays_When_Post_Is_Invoked_Should_Send_Request_With_Same_TicketId()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[] { new CreateTicketPlaysInput(LineIndex: 1, Numbers: [1, 2, 3, 4, 5]) };

        _mediator
            .Send(Arg.Any<AddTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<Guid>>.Failure("failed"));

        // Act
        await _sut.Post(ticketId, plays);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<AddTicketPlaysRequest>(r => r.TicketId == ticketId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_TicketId_And_Plays_When_Post_Is_Invoked_Should_Send_Request_With_Same_Plays_Instance()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[] { new CreateTicketPlaysInput(LineIndex: 1, Numbers: [1, 2, 3, 4, 5]) };

        _mediator
            .Send(Arg.Any<AddTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<Guid>>.Failure("failed"));

        // Act
        await _sut.Post(ticketId, plays);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<AddTicketPlaysRequest>(r => ReferenceEquals(r.Plays, plays)),
            Arg.Any<CancellationToken>());
    }
}