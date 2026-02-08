using FluentAssertions;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.RepositoriesTests.TicketRepositoryTests;

[TestFixture]
public class GetAllUserTicketsTests
{
    [Test]
    public async Task Given_No_Tickets_When_GetAllUserTickets_Is_Invoked_Should_Return_Empty()
    {
        // Arrange
        var (sut, _) = CreateSut();
        var userId = Guid.NewGuid();

        // Act
        var result = await sut.GetAllUserTickets(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task Given_Tickets_For_Different_Users_When_GetAllUserTickets_Is_Invoked_Should_Return_Only_User_Tickets()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var userId = Guid.NewGuid();

        await Seed(options,
            CreateTicket(id: Guid.NewGuid(), userId: userId, name: "Mine"),
            CreateTicket(id: Guid.NewGuid(), userId: Guid.NewGuid(), name: "Not mine"));

        // Act
        var result = await sut.GetAllUserTickets(userId);

        // Assert
        result.Length.Should().Be(1);
    }

    [Test]
    public async Task Given_User_Ticket_When_GetAllUserTickets_Is_Invoked_Should_Map_Ticket_Name()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var userId = Guid.NewGuid();

        await Seed(options, CreateTicket(id: Guid.NewGuid(), userId: userId, name: "My Ticket"));

        // Act
        var result = await sut.GetAllUserTickets(userId);

        // Assert
        result.Single().Name.Should().Be("My Ticket");
    }

    [Test]
    public async Task Given_User_Ticket_With_Plays_When_GetAllUserTickets_Is_Invoked_Should_Map_Play_Count()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var userId = Guid.NewGuid();

        var ticket = CreateTicket(id: Guid.NewGuid(), userId: userId, name: "My Ticket");
        ticket.TicketPlays = new List<TicketPlay>
            {
                CreatePlay(id: Guid.NewGuid(), ticketId: ticket.Id, lineIndex: 1, numbers: new List<int> { 1,2,3 }),
                CreatePlay(id: Guid.NewGuid(), ticketId: ticket.Id, lineIndex: 2, numbers: new List<int> { 4,5,6 }),
            };

        await Seed(options, ticket);

        // Act
        var result = await sut.GetAllUserTickets(userId);

        // Assert
        result.Single().UserTicketPlays.Count.Should().Be(2);
    }

    [Test]
    public async Task Given_User_Ticket_With_Play_When_GetAllUserTickets_Is_Invoked_Should_Map_Play_LineIndex()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var userId = Guid.NewGuid();

        var ticket = CreateTicket(id: Guid.NewGuid(), userId: userId, name: "My Ticket");
        ticket.TicketPlays = new List<TicketPlay>
            {
                CreatePlay(id: Guid.NewGuid(), ticketId: ticket.Id, lineIndex: 7, numbers: new List<int> { 1,2,3 })
            };

        await Seed(options, ticket);

        // Act
        var result = await sut.GetAllUserTickets(userId);

        // Assert
        result.Single().UserTicketPlays.Single().LineIndex.Should().Be(7);
    }

    [Test]
    public async Task Given_User_Ticket_With_Play_When_GetAllUserTickets_Is_Invoked_Should_Map_Play_Numbers()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var userId = Guid.NewGuid();

        var numbers = new List<int> { 3, 8, 13, 21, 34 };

        var ticket = CreateTicket(id: Guid.NewGuid(), userId: userId, name: "My Ticket");
        ticket.TicketPlays = new List<TicketPlay>
            {
                CreatePlay(id: Guid.NewGuid(), ticketId: ticket.Id, lineIndex: 1, numbers: numbers)
            };

        await Seed(options, ticket);

        // Act
        var result = await sut.GetAllUserTickets(userId);

        // Assert
        result.Single().UserTicketPlays.Single().Numbers.Should().Equal(numbers);
    }

    private static (TicketRepository sut, DbContextOptions<LotteryDbContext> options) CreateSut()
    {
        var dbName = $"TicketRepo-GetAllUserTickets-{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<LotteryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factory = Substitute.For<IDbContextFactory<LotteryDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
               .Returns(_ => new LotteryDbContext(options));

        return (new TicketRepository(factory), options);
    }

    private static async Task Seed(DbContextOptions<LotteryDbContext> options, params Ticket[] tickets)
    {
        await using var context = new LotteryDbContext(options);
        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();
    }

    private static Ticket CreateTicket(Guid id, Guid userId, string name)
    {
        return new Ticket
        {
            Id = id,
            UserId = userId,
            LotteryId = 1,
            Name = name,
            Description = "desc",
            IsPublic = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            TicketPlays = new List<TicketPlay>()
        };
    }

    private static TicketPlay CreatePlay(Guid id, Guid ticketId, int lineIndex, List<int> numbers)
    {
        return new TicketPlay
        {
            Id = id,
            TicketId = ticketId,
            LineIndex = lineIndex,
            Numbers = numbers,
            CreatedAt = DateTime.UtcNow
        };
    }
}