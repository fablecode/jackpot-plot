using FluentAssertions;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.RepositoriesTests.TicketRepositoryTests;

[TestFixture]
public class GetTicketByIdTests
{
    [Test]
    public async Task Given_No_Tickets_When_GetTicketById_Is_Invoked_Should_Return_Null()
    {
        // Arrange
        var (sut, _) = CreateSut();
        var ticketId = Guid.NewGuid();

        // Act
        var result = await sut.GetTicketById(ticketId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Given_Existing_Ticket_When_GetTicketById_Is_Invoked_Should_Return_Ticket_With_Same_Id()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        await Seed(options, CreateTicket(ticketId));

        // Act
        var result = await sut.GetTicketById(ticketId);

        // Assert
        result!.Id.Should().Be(ticketId);
    }

    [Test]
    public async Task Given_Existing_Ticket_When_GetTicketById_Is_Invoked_Should_Map_Name()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        await Seed(options, CreateTicket(ticketId, name: "My Ticket"));

        // Act
        var result = await sut.GetTicketById(ticketId);

        // Assert
        result!.Name.Should().Be("My Ticket");
    }

    [Test]
    public async Task Given_Existing_Ticket_With_Plays_When_GetTicketById_Is_Invoked_Should_Map_Play_Count()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        var ticket = CreateTicket(ticketId);
        ticket.TicketPlays = new List<TicketPlay>
            {
                CreatePlay(Guid.NewGuid(), ticketId, lineIndex: 1, numbers: new List<int> { 1, 2, 3 }),
                CreatePlay(Guid.NewGuid(), ticketId, lineIndex: 2, numbers: new List<int> { 4, 5, 6 }),
            };

        await Seed(options, ticket);

        // Act
        var result = await sut.GetTicketById(ticketId);

        // Assert
        result!.UserTicketPlays.Count.Should().Be(2);
    }

    [Test]
    public async Task Given_Existing_Ticket_With_Play_When_GetTicketById_Is_Invoked_Should_Map_Play_LineIndex()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        var ticket = CreateTicket(ticketId);
        ticket.TicketPlays = new List<TicketPlay>
            {
                CreatePlay(Guid.NewGuid(), ticketId, lineIndex: 7, numbers: new List<int> { 1, 2, 3 })
            };

        await Seed(options, ticket);

        // Act
        var result = await sut.GetTicketById(ticketId);

        // Assert
        result!.UserTicketPlays.Single().LineIndex.Should().Be(7);
    }

    [Test]
    public async Task Given_Existing_Ticket_With_Play_When_GetTicketById_Is_Invoked_Should_Map_Play_Numbers()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        var numbers = new List<int> { 3, 8, 13, 21, 34 };

        var ticket = CreateTicket(ticketId);
        ticket.TicketPlays = new List<TicketPlay>
            {
                CreatePlay(Guid.NewGuid(), ticketId, lineIndex: 1, numbers: numbers)
            };

        await Seed(options, ticket);

        // Act
        var result = await sut.GetTicketById(ticketId);

        // Assert
        result!.UserTicketPlays.Single().Numbers.Should().Equal(numbers);
    }

    private static (TicketRepository sut, DbContextOptions<LotteryDbContext> options) CreateSut()
    {
        var dbName = $"TicketRepo-GetTicketById-{Guid.NewGuid()}";

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

    private static Ticket CreateTicket(Guid id, string name = "Ticket")
    {
        return new Ticket
        {
            Id = id,
            UserId = Guid.NewGuid(),
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