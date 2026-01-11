using FluentAssertions;
using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Features.DeleteTicketPlays;
using Lottery.API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Lottery.API.Unit.Tests.ControllersTests.PlaysControllerTests;

[TestFixture]
public class DeleteTests
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
    public async Task Given_Success_Result_When_Delete_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _mediator
            .Send(Arg.Any<DeleteTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _sut.Delete(ticketId, playIds);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_Failure_Result_When_Delete_Is_Invoked_Should_Return_BadRequest()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid() };

        _mediator
            .Send(Arg.Any<DeleteTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Failure("failed"));

        // Act
        var result = await _sut.Delete(ticketId, playIds);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Given_TicketId_When_Delete_Is_Invoked_Should_Send_Request_With_Same_TicketId()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _mediator
            .Send(Arg.Any<DeleteTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Failure("failed"));

        // Act
        await _sut.Delete(ticketId, playIds);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<DeleteTicketPlaysRequest>(r => r.TicketId == ticketId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_PlayIds_When_Delete_Is_Invoked_Should_Send_Request_With_Same_PlayIds_Instance()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _mediator
            .Send(Arg.Any<DeleteTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Failure("failed"));

        // Act
        await _sut.Delete(ticketId, playIds);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<DeleteTicketPlaysRequest>(r => ReferenceEquals(r.PlayIds, playIds)),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Request_When_Delete_Is_Invoked_Should_Send_To_Mediator_Once()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid() };

        _mediator
            .Send(Arg.Any<DeleteTicketPlaysRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Failure("failed"));

        // Act
        await _sut.Delete(ticketId, playIds);

        // Assert
        await _mediator.Received(1).Send(Arg.Any<DeleteTicketPlaysRequest>(), Arg.Any<CancellationToken>());
    }
}