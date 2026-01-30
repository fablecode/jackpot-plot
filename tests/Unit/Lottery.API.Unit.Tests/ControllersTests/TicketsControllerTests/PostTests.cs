using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Features.CreateUserTicket;
using JackpotPlot.Lottery.API.Application.Models.Input;
using Lottery.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using System.Security.Claims;
using MediatR;

namespace Lottery.API.Unit.Tests.ControllersTests.TicketsControllerTests;

[TestFixture]
public class PostTests
{
    private IMediator _mediator;
    private TicketsController _sut;

    [SetUp]
    public void SetUp()
    {
        _mediator = Substitute.For<IMediator>();

        _sut = new TicketsController(_mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task Given_Success_Result_When_Post_Is_Invoked_Should_Return_CreatedAtAction()
    {
        // Arrange
        SetUserWithNameIdentifier(Guid.NewGuid());
        var input = CreateInput();

        _mediator
            .Send(Arg.Any<CreateUserTicketRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<CreateUserTicketResponse>.Success(new CreateUserTicketResponse(Guid.NewGuid())));

        // Act
        var result = await _sut.Post(input);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Test]
    public async Task Given_Success_Result_When_Post_Is_Invoked_Should_Set_Route_Id_To_Created_Id()
    {
        // Arrange
        SetUserWithNameIdentifier(Guid.NewGuid());
        var input = CreateInput();
        var createdId = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<CreateUserTicketRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<CreateUserTicketResponse>.Success(new CreateUserTicketResponse(createdId)));

        // Act
        var result = (CreatedAtActionResult)await _sut.Post(input);

        // Assert
        result.RouteValues!["id"].Should().Be(createdId);
    }

    [Test]
    public async Task Given_Failure_Result_When_Post_Is_Invoked_Should_Return_BadRequest()
    {
        // Arrange
        SetUserWithNameIdentifier(Guid.NewGuid());
        var input = CreateInput();

        _mediator
            .Send(Arg.Any<CreateUserTicketRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<CreateUserTicketResponse>.Failure("failed"));

        // Act
        var result = await _sut.Post(input);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Given_NameIdentifier_Claim_When_Post_Is_Invoked_Should_Send_Request_With_Same_UserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserWithNameIdentifier(userId);
        var input = CreateInput();

        _mediator
            .Send(Arg.Any<CreateUserTicketRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<CreateUserTicketResponse>.Failure("failed"));

        // Act
        await _sut.Post(input);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateUserTicketRequest>(r => r.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Sub_Claim_When_Post_Is_Invoked_Should_Send_Request_With_Same_UserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserWithSub(userId);
        var input = CreateInput();

        _mediator
            .Send(Arg.Any<CreateUserTicketRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<CreateUserTicketResponse>.Failure("failed"));

        // Act
        await _sut.Post(input);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateUserTicketRequest>(r => r.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Input_When_Post_Is_Invoked_Should_Send_Request_With_Same_Ticket_Instance()
    {
        // Arrange
        SetUserWithNameIdentifier(Guid.NewGuid());
        var input = CreateInput();

        _mediator
            .Send(Arg.Any<CreateUserTicketRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<CreateUserTicketResponse>.Failure("failed"));

        // Act
        await _sut.Post(input);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateUserTicketRequest>(r => ReferenceEquals(r.Ticket, input)),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void Given_Missing_UserId_Claim_When_Post_Is_Invoked_Should_Throw_UnauthorizedAccessException()
    {
        // Arrange
        _sut.ControllerContext.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity());
        var input = CreateInput();

        // Act
        Func<Task> act = async () => await _sut.Post(input);

        // Assert
        act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private void SetUserWithNameIdentifier(Guid userId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
            authenticationType: "TestAuth");

        _sut.ControllerContext.HttpContext!.User = new ClaimsPrincipal(identity);
    }

    private void SetUserWithSub(Guid userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim("sub", userId.ToString())],
            authenticationType: "TestAuth");

        _sut.ControllerContext.HttpContext!.User = new ClaimsPrincipal(identity);
    }

    private static CreateTicketInput CreateInput()
    {
        return new CreateTicketInput(
            name: "My Ticket",
            plays:
            [
                new CreateTicketPlaysInput(LineIndex: 1, Numbers: [1, 2, 3, 4, 5]),
                    new CreateTicketPlaysInput(LineIndex: 2, Numbers: [6, 7, 8, 9, 10])
            ])
        {
            LotteryId = 7
        };
    }
}