using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Features.GetTickets;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.GetTicketsQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private ITicketRepository _ticketRepository;
    private GetTicketsQueryHandler _sut;

    [SetUp]
    public void SetUp()
    {
        _ticketRepository = Substitute.For<ITicketRepository>();
        _sut = new GetTicketsQueryHandler(_ticketRepository);
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Call_SearchTickets_With_Request_Parameters()
    {
        // Arrange
        var request = new GetTicketsQuery(
            PageNumber: 2,
            PageSize: 25,
            UserId: Guid.NewGuid(),
            SearchTerm: "test",
            SortColumn: "ticket_name",
            SortDirection: "desc");

        var pagedTickets = new PagedTickets();

        _ticketRepository
            .SearchTickets(
                request.PageNumber,
                request.PageSize,
                request.UserId,
                request.SearchTerm,
                request.SortColumn,
                request.SortDirection)
            .Returns(Task.FromResult(pagedTickets));

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _ticketRepository.Received(1).SearchTickets(
            request.PageNumber,
            request.PageSize,
            request.UserId,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection);
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var request = new GetTicketsQuery();
        var pagedTickets = new PagedTickets();

        _ticketRepository
            .SearchTickets(
                request.PageNumber,
                request.PageSize,
                request.UserId,
                request.SearchTerm,
                request.SortColumn,
                request.SortDirection)
            .Returns(Task.FromResult(pagedTickets));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Return_PagedTickets_As_Value()
    {
        // Arrange
        var request = new GetTicketsQuery();
        var pagedTickets = new PagedTickets();

        _ticketRepository
            .SearchTickets(
                request.PageNumber,
                request.PageSize,
                request.UserId,
                request.SearchTerm,
                request.SortColumn,
                request.SortDirection)
            .Returns(Task.FromResult(pagedTickets));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Value.Should().Be(pagedTickets);
    }
}