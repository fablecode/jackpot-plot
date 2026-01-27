using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Features.GetAllUserTickets;
using JackpotPlot.Lottery.API.Application.Features.GetTicketById;
using JackpotPlot.Lottery.API.Application.Features.GetTickets;
using JackpotPlot.Lottery.API.Application.Models.Output;
using Lottery.API.Controllers;
using Lottery.API.Models.Input;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using System.Security.Claims;
using FluentAssertions;
using MediatR;

namespace Lottery.API.Unit.Tests.ControllersTests.TicketsControllerTests;

[TestFixture]
public class GetTests
{
    private IMediator _mediator;
    private TicketsController _sut;

    [SetUp]
    public void SetUp()
    {
        _mediator = Substitute.For<IMediator>();
        _sut = new TicketsController(_mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    // -------------------------
    // GET api/tickets  (authorized)
    // -------------------------

    [Test]
    public async Task Given_Authenticated_User_When_GetAll_Is_Invoked_Should_Return_Ok_On_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(authenticated: true, userId);

        var output = ImmutableArray.Create(new TicketOutput(Guid.NewGuid(), "T1", true, 0));

        _mediator
            .Send(Arg.Any<GetAllUserTicketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<TicketOutput>>.Success(output));

        // Act
        var result = await _sut.Get();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Authenticated_User_When_GetAll_Is_Invoked_Should_Send_Query_With_UserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(authenticated: true, userId);

        _mediator
            .Send(Arg.Any<GetAllUserTicketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<TicketOutput>>.Failure("failed"));

        // Act
        await _sut.Get();

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetAllUserTicketsQuery>(q => q.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Authenticated_User_When_GetAll_Is_Invoked_Should_Return_NoContent_On_Failure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(authenticated: true, userId);

        _mediator
            .Send(Arg.Any<GetAllUserTicketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ImmutableArray<TicketOutput>>.Failure("failed"));

        // Act
        var result = await _sut.Get();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // -------------------------
    // GET api/tickets/{id:guid}
    // -------------------------

    [Test]
    public async Task Given_Existing_Ticket_When_GetById_Is_Invoked_Should_Return_Ok_On_Success()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var output = new TicketOutput(ticketId, "T1", true, 3);

        _mediator
            .Send(Arg.Any<GetTicketByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TicketOutput>.Success(output));

        // Act
        var result = await _sut.Get(ticketId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_TicketId_When_GetById_Is_Invoked_Should_Send_Query_With_Same_Id()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<GetTicketByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TicketOutput>.Failure("not found"));

        // Act
        await _sut.Get(ticketId);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetTicketByIdQuery>(q => q.Id == ticketId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Missing_Ticket_When_GetById_Is_Invoked_Should_Return_NoContent_On_Failure()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        _mediator
            .Send(Arg.Any<GetTicketByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TicketOutput>.Failure("not found"));

        // Act
        var result = await _sut.Get(ticketId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // -------------------------
    // GET api/tickets/search (allow anonymous)
    // -------------------------

    [Test]
    public async Task Given_Anonymous_User_When_Search_Is_Invoked_Should_Send_Query_With_Null_UserId()
    {
        // Arrange
        SetAnonymous();

        var input = new TicketOverviewInput { PageNumber = 2, PageSize = 25, SearchTerm = "x", SortColumn = "ticket_id", SortDirection = "asc" };

        _mediator
            .Send(Arg.Any<GetTicketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedTickets>.Failure("failed"));

        // Act
        await _sut.Get(input);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetTicketsQuery>(q => q.UserId == null),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Authenticated_User_With_Valid_Guid_Claim_When_Search_Is_Invoked_Should_Send_Query_With_UserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(authenticated: true, userId);

        var input = new TicketOverviewInput { PageNumber = 1, PageSize = 10 };

        _mediator
            .Send(Arg.Any<GetTicketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedTickets>.Failure("failed"));

        // Act
        await _sut.Get(input);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<GetTicketsQuery>(q => q.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Success_Result_When_Search_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        SetAnonymous();

        var input = new TicketOverviewInput();
        var paged = new PagedTickets { TotalItems = 1, TotalFilteredItems = 1, TotalPages = 1 };

        _mediator
            .Send(Arg.Any<GetTicketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedTickets>.Success(paged));

        // Act
        var result = await _sut.Get(input);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Given_Failure_Result_When_Search_Is_Invoked_Should_Return_NoContent()
    {
        // Arrange
        SetAnonymous();

        var input = new TicketOverviewInput();

        _mediator
            .Send(Arg.Any<GetTicketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedTickets>.Failure("failed"));

        // Act
        var result = await _sut.Get(input);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    private void SetAnonymous()
    {
        _sut.ControllerContext.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity());
    }

    private void SetUser(bool authenticated, Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = authenticated ? new ClaimsIdentity(claims, "TestAuth") : new ClaimsIdentity();
        _sut.ControllerContext.HttpContext!.User = new ClaimsPrincipal(identity);
    }
}