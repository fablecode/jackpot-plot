using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Features.GetTicketPlayById;
using Lottery.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;
using MediatR;

namespace Lottery.API.Unit.Tests.ControllersTests.PlaysControllerTests;

[TestFixture]
public class GetTests
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
    public async Task Given_Success_Result_When_Get_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = ImmutableArray.Create(new PlayLine([1, 2, 3, 4, 5]));

        _mediator
            .Send(Arg.Any<GetTicketPlayByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<PlayLine>>.Success(plays));

        // Act
        var result = await _sut.Get(ticketId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_Get_Is_Invoked_Should_Return_Plays_As_Ok_Value()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = ImmutableArray.Create(new PlayLine([1, 2, 3, 4, 5]));

        _mediator
            .Send(Arg.Any<GetTicketPlayByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<PlayLine>>.Success(plays));

        // Act
        var result = await _sut.Get(ticketId);

        // Assert
        ((OkObjectResult)result).Value.Should().Be(plays);
    }

    [Test]
    public async Task Given_Failure_Result_When_Get_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<GetTicketPlayByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<PlayLine>>.Failure("not found"));

        // Act
        var result = await _sut.Get(ticketId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Given_TicketId_When_Get_Is_Invoked_Should_Send_Query_With_Same_TicketId()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<GetTicketPlayByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<PlayLine>>.Failure("not found"));

        // Act
        await _sut.Get(ticketId);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetTicketPlayByIdQuery>(q => q.Id == ticketId),
            Arg.Any<CancellationToken>());
    }
}