using FluentAssertions;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.RepositoriesTests.LotteryRepositoryTests;

[TestFixture]
public class GetLotteryIdByNameTests
{
    [Test]
    public async Task Given_No_Lotteries_When_GetLotteryIdByName_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var (sut, _) = CreateSut();

        // Act
        var id = await sut.GetLotteryIdByName("Eurojackpot");

        // Assert
        id.Should().Be(0);
    }

    [Test]
    public async Task Given_Matching_Lottery_When_GetLotteryIdByName_Is_Invoked_Should_Return_Lottery_Id()
    {
        // Arrange
        var (sut, options) = CreateSut();

        await Seed(options, new Models.Lottery
        {
            Id = 7,
            Name = "Eurojackpot"
        });

        // Act
        var id = await sut.GetLotteryIdByName("Eurojackpot");

        // Assert
        id.Should().Be(7);
    }

    [Test]
    public async Task Given_Name_With_Different_Casing_When_GetLotteryIdByName_Is_Invoked_Should_Return_Lottery_Id()
    {
        // Arrange
        var (sut, options) = CreateSut();

        await Seed(options, new Models.Lottery
        {
            Id = 7,
            Name = "Eurojackpot"
        });

        // Act
        var id = await sut.GetLotteryIdByName("EUROJACKPOT");

        // Assert
        id.Should().Be(7);
    }

    [Test]
    public async Task Given_Non_Matching_Name_When_GetLotteryIdByName_Is_Invoked_Should_Return_Zero()
    {
        // Arrange
        var (sut, options) = CreateSut();

        await Seed(options, new Models.Lottery
        {
            Id = 7,
            Name = "Eurojackpot"
        });

        // Act
        var id = await sut.GetLotteryIdByName("Powerball");

        // Assert
        id.Should().Be(0);
    }

    private static (LotteryRepository sut, DbContextOptions<LotteryDbContext> options) CreateSut()
    {
        var dbName = $"LotteryRepoTests-{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<LotteryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factory = Substitute.For<IDbContextFactory<LotteryDbContext>>();
        factory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => new LotteryDbContext(options));

        return (new LotteryRepository(factory), options);
    }

    private static async Task Seed(
        DbContextOptions<LotteryDbContext> options,
        params Models.Lottery[] lotteries)
    {
        await using var context = new LotteryDbContext(options);
        context.Lotteries.AddRange(lotteries);
        await context.SaveChangesAsync();
    }
}