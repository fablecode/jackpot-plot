using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.RepositoriesTests.TicketRepositoryTests;

[TestFixture]
public class AddTests
{
    [Test]
    public async Task Given_Valid_Ticket_When_Add_Is_Invoked_Should_Return_NonEmpty_Id()
    {
        // Arrange
        var (sut, _) = CreateSut();
        var ticket = CreateTicketDomain(withPlays: false);

        // Act
        var id = await sut.Add(ticket);

        // Assert
        id.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task Given_Ticket_When_Add_Is_Invoked_Should_Persist_One_Ticket()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticket = CreateTicketDomain(withPlays: false);

        // Act
        await sut.Add(ticket);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.Tickets.Count().Should().Be(1);
    }

    [Test]
    public async Task Given_Ticket_When_Add_Is_Invoked_Should_Persist_Same_UserId()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticket = CreateTicketDomain(withPlays: false);

        // Act
        await sut.Add(ticket);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.Tickets.Single().UserId.Should().Be(ticket.UserId);
    }

    [Test]
    public async Task Given_Ticket_When_Add_Is_Invoked_Should_Persist_Same_LotteryId()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticket = CreateTicketDomain(withPlays: false);

        // Act
        await sut.Add(ticket);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.Tickets.Single().LotteryId.Should().Be(ticket.LotteryId);
    }

    [Test]
    public async Task Given_Ticket_When_Add_Is_Invoked_Should_Persist_Same_Name()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticket = CreateTicketDomain(withPlays: false);

        // Act
        await sut.Add(ticket);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.Tickets.Single().Name.Should().Be(ticket.Name);
    }

    [Test]
    public async Task Given_Ticket_With_Plays_When_Add_Is_Invoked_Should_Persist_Play_Count()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticket = CreateTicketDomain(withPlays: true);

        // Act
        await sut.Add(ticket);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.TicketPlays.Count().Should().Be(ticket.UserTicketPlays.Count);
    }

    private static (TicketRepository sut, DbContextOptions<LotteryDbContext> options) CreateSut()
    {
        var dbName = $"TicketRepo-Add-{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<LotteryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factory = Substitute.For<IDbContextFactory<LotteryDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
               .Returns(_ => new LotteryDbContext(options));

        return (new TicketRepository(factory), options);
    }

    private static TicketDomain CreateTicketDomain(bool withPlays)
    {
        return new TicketDomain
        {
            Id = Guid.Empty, // repo should generate/assign
            UserId = Guid.NewGuid(),
            LotteryId = 7,
            Name = "My Ticket",
            Description = "desc",
            IsPublic = true,
            IsDeleted = false,
            UserTicketPlays = withPlays
                ? new List<TicketPlayDomain>
                {
                        new() { LineIndex = 1, Numbers = new List<int> { 1, 2, 3, 4, 5 } },
                        new() { LineIndex = 2, Numbers = new List<int> { 6, 7, 8, 9, 10 } }
                }
                : new List<TicketPlayDomain>()
        };
    }
}