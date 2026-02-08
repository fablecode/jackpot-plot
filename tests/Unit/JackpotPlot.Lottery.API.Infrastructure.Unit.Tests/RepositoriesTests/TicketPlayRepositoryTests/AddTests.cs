using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.RepositoriesTests.TicketPlayRepositoryTests;

[TestFixture]
public class AddTests
{
    [Test]
    public async Task Given_TicketPlays_When_Add_Is_Invoked_Should_Persist_All_Plays()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        var plays = ImmutableArray.Create(
            (LineIndex: 1, Numbers: [1, 2, 3, 4, 5]),
            (LineIndex: 2, Numbers: new List<int> { 6, 7, 8, 9, 10 })
        );

        // Act
        await sut.Add(ticketId, plays);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.TicketPlays.Count().Should().Be(2);
    }

    [Test]
    public async Task Given_TicketPlays_When_Add_Is_Invoked_Should_Return_Same_Number_Of_Ids()
    {
        // Arrange
        var (sut, _) = CreateSut();
        var ticketId = Guid.NewGuid();

        var plays = ImmutableArray.Create(
            (LineIndex: 1, Numbers: [1, 2, 3, 4, 5]),
            (LineIndex: 2, Numbers: [6, 7, 8, 9, 10]),
            (LineIndex: 3, Numbers: new List<int> { 11, 12, 13, 14, 15 })
        );

        // Act
        var ids = await sut.Add(ticketId, plays);

        // Assert
        ids.Length.Should().Be(plays.Length);
    }

    [Test]
    public async Task Given_TicketPlays_When_Add_Is_Invoked_Should_Return_NonEmpty_Ids()
    {
        // Arrange
        var (sut, _) = CreateSut();
        var ticketId = Guid.NewGuid();

        var plays = ImmutableArray.Create(
            (LineIndex: 1, Numbers: new List<int> { 1, 2, 3, 4, 5 })
        );

        // Act
        var ids = await sut.Add(ticketId, plays);

        // Assert
        ids.Single().Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task Given_TicketId_When_Add_Is_Invoked_Should_Persist_TicketId_On_All_Plays()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        var plays = ImmutableArray.Create(
            (LineIndex: 1, Numbers: [1, 2, 3, 4, 5]),
            (LineIndex: 2, Numbers: new List<int> { 6, 7, 8, 9, 10 })
        );

        // Act
        await sut.Add(ticketId, plays);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.TicketPlays.All(p => p.TicketId == ticketId).Should().BeTrue();
    }

    [Test]
    public async Task Given_LineIndex_When_Add_Is_Invoked_Should_Persist_LineIndex()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        var plays = ImmutableArray.Create(
            (LineIndex: 7, Numbers: new List<int> { 1, 2, 3, 4, 5 })
        );

        // Act
        await sut.Add(ticketId, plays);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.TicketPlays.Single().LineIndex.Should().Be(7);
    }

    [Test]
    public async Task Given_Numbers_When_Add_Is_Invoked_Should_Persist_Numbers()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var ticketId = Guid.NewGuid();

        var numbers = new List<int> { 3, 8, 13, 21, 34 };

        var plays = ImmutableArray.Create(
            (LineIndex: 1, Numbers: numbers)
        );

        // Act
        await sut.Add(ticketId, plays);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.TicketPlays.Single().Numbers.Should().Equal(numbers);
    }

    private static (TicketPlayRepository sut, DbContextOptions<LotteryDbContext> options) CreateSut()
    {
        var dbName = $"TicketPlayRepoTests-{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<LotteryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factory = Substitute.For<IDbContextFactory<LotteryDbContext>>();
        factory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => new LotteryDbContext(options));

        return (new TicketPlayRepository(factory), options);
    }
}