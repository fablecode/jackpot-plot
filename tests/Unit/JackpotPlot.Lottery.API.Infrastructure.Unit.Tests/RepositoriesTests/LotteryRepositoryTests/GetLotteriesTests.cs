using FluentAssertions;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.RepositoriesTests.LotteryRepositoryTests;

[TestFixture]
public class GetLotteriesTests
{
    [Test]
    public async Task Given_No_Lotteries_When_GetLotteries_Is_Invoked_Should_Return_Empty_Collection()
    {
        // Arrange
        var (sut, _) = CreateSut();

        // Act
        var result = await sut.GetLotteries();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task Given_Lotteries_When_GetLotteries_Is_Invoked_Should_Return_All_Items()
    {
        // Arrange
        var (sut, options) = CreateSut();

        await Seed(options,
            new Models.Lottery { Id = 1, Name = "Eurojackpot" },
            new Models.Lottery { Id = 2, Name = "Powerball" });

        // Act
        var result = await sut.GetLotteries();

        // Assert
        result.Count.Should().Be(2);
    }

    [Test]
    public async Task Given_Lotteries_When_GetLotteries_Is_Invoked_Should_Map_Id_And_Name()
    {
        // Arrange
        var (sut, options) = CreateSut();

        await Seed(options,
            new Models.Lottery { Id = 7, Name = "Eurojackpot" },
            new Models.Lottery { Id = 9, Name = "Mega Millions" });

        var expected = new List<LotteryDomain>
            {
                new() { Id = 7, Name = "Eurojackpot" },
                new() { Id = 9, Name = "Mega Millions" }
            };

        // Act
        var result = await sut.GetLotteries();

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    private static (LotteryRepository sut, DbContextOptions<LotteryDbContext> options) CreateSut()
    {
        var dbName = $"LotteryRepo-GetLotteries-{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<LotteryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factory = Substitute.For<IDbContextFactory<LotteryDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
               .Returns(_ => new LotteryDbContext(options));

        return (new LotteryRepository(factory), options);
    }

    private static async Task Seed(DbContextOptions<LotteryDbContext> options, params Models.Lottery[] lotteries)
    {
        await using var context = new LotteryDbContext(options);
        context.Lotteries.AddRange(lotteries);
        await context.SaveChangesAsync();
    }
}