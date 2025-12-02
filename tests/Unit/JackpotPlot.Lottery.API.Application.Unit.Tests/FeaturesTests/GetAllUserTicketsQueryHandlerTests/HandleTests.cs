using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Application.Features.GetAllUserTickets;
using JackpotPlot.Lottery.API.Application.Models.Output;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.GetAllUserTicketsQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private ITicketRepository _ticketRepository = null!;
    private GetAllUserTicketsQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _ticketRepository = Substitute.For<ITicketRepository>();
        _sut = new GetAllUserTicketsQueryHandler(_ticketRepository);
    }


    // --------------------------------------------------------------------
    // Repository interaction
    // --------------------------------------------------------------------

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_Repository_With_UserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _ticketRepository
            .GetAllUserTickets(userId)
            .Returns(Task.FromResult<ImmutableArray<TicketDomain>>([]));

        var query = new GetAllUserTicketsQuery(userId);

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _ticketRepository.Received(1).GetAllUserTickets(userId);
    }

    // --------------------------------------------------------------------
    // Result mapping
    // --------------------------------------------------------------------

    [Test]
    public async Task Given_Tickets_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tickets = new[]
        {
                CreateTicket(userId, "A", true, 2),
                CreateTicket(userId, "B", false, 3)
            };

        _ticketRepository
            .GetAllUserTickets(userId)
            .Returns(Task.FromResult<ImmutableArray<TicketDomain>>(tickets));

        var query = new GetAllUserTicketsQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Tickets_When_Handle_Is_Invoked_Should_Map_Each_Ticket_To_TicketOutput()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticket = CreateTicket(userId, "My ticket", true, 4);

        _ticketRepository
            .GetAllUserTickets(userId)
            .Returns(Task.FromResult<ImmutableArray<TicketDomain>>([ticket]));

        var query = new GetAllUserTicketsQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo([
            new TicketOutput(
                    Id: ticket.Id,
                    Name: ticket.Name,
                    IsPublic: ticket.IsPublic,
                    PlayCount: ticket.UserTicketPlays.Count)
        ]);
    }

    [Test]
    public async Task Given_No_Tickets_When_Handle_Is_Invoked_Should_Return_Empty_Array()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _ticketRepository
            .GetAllUserTickets(userId)
            .Returns(Task.FromResult<ImmutableArray<TicketDomain>>([]));

        var query = new GetAllUserTicketsQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEmpty();
    }

    // Test Helpers
    private static TicketDomain CreateTicket(Guid userId, string name, bool isPublic, int playCount)
    {
        var ticket = new TicketDomain
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            IsPublic = isPublic
        };

        for (var i = 0; i < playCount; i++)
        {
            ticket.UserTicketPlays.Add(new TicketPlayDomain { Ticket = ticket });
        }

        return ticket;
    }

}