using FluentAssertions;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.RepositoriesTests.LotteryConfigurationRepositoryTests;

[TestFixture]
public class GetActiveConfigurationAsyncTests
{
    [Test]
    public async Task Given_No_Configurations_When_GetActiveConfigurationAsync_Is_Invoked_Should_Return_Null()
    {
        // Arrange
        var (sut, _) = CreateSut();

        // Act
        var result = await sut.GetActiveConfigurationAsync(lotteryId: 7);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Given_Only_Expired_Configuration_When_GetActiveConfigurationAsync_Is_Invoked_Should_Return_Null()
    {
        // Arrange
        var (sut, options) = CreateSut();
        await Seed(options, new LotteryConfiguration
        {
            Id = 1,
            LotteryId = 7,
            StartDate = DateTime.Now.AddDays(-10),
            EndDate = DateTime.Now.AddSeconds(-1),
            MainNumbersCount = 5,
            MainNumbersRange = 50
        });

        // Act
        var result = await sut.GetActiveConfigurationAsync(lotteryId: 7);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Given_Configuration_For_Different_Lottery_When_GetActiveConfigurationAsync_Is_Invoked_Should_Return_Null()
    {
        // Arrange
        var (sut, options) = CreateSut();
        await Seed(options, new LotteryConfiguration
        {
            Id = 1,
            LotteryId = 999,
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = null,
            MainNumbersCount = 5,
            MainNumbersRange = 50
        });

        // Act
        var result = await sut.GetActiveConfigurationAsync(lotteryId: 7);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Given_Active_Configuration_When_GetActiveConfigurationAsync_Is_Invoked_Should_Return_Matching_LotteryId()
    {
        // Arrange
        var (sut, options) = CreateSut();
        await Seed(options, new LotteryConfiguration
        {
            Id = 1,
            LotteryId = 7,
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = null,
            MainNumbersCount = 5,
            MainNumbersRange = 50
        });

        // Act
        var result = await sut.GetActiveConfigurationAsync(lotteryId: 7);

        // Assert
        result!.LotteryId.Should().Be(7);
    }

    [Test]
    public async Task Given_Two_Active_Configurations_When_GetActiveConfigurationAsync_Is_Invoked_Should_Return_Most_Recent_StartDate()
    {
        // Arrange
        var (sut, options) = CreateSut();

        await Seed(options,
            new LotteryConfiguration
            {
                Id = 1,
                LotteryId = 7,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = null,
                MainNumbersCount = 5,
                MainNumbersRange = 50
            },
            new LotteryConfiguration
            {
                Id = 2,
                LotteryId = 7,
                StartDate = DateTime.Now.AddDays(-1), // more recent
                EndDate = null,
                MainNumbersCount = 5,
                MainNumbersRange = 50
            });

        // Act
        var result = await sut.GetActiveConfigurationAsync(lotteryId: 7);

        // Assert
        result!.Id.Should().Be(2);
    }

    [Test]
    public async Task Given_Same_StartDate_When_GetActiveConfigurationAsync_Is_Invoked_Should_Return_Soonest_EndDate()
    {
        // Arrange
        var (sut, options) = CreateSut();
        var sameStart = DateTime.Now.AddDays(-5);

        await Seed(options,
            new LotteryConfiguration
            {
                Id = 1,
                LotteryId = 7,
                StartDate = sameStart,
                EndDate = DateTime.Now.AddDays(10), // later end
                MainNumbersCount = 5,
                MainNumbersRange = 50
            },
            new LotteryConfiguration
            {
                Id = 2,
                LotteryId = 7,
                StartDate = sameStart,
                EndDate = DateTime.Now.AddDays(1), // sooner end => should win
                MainNumbersCount = 5,
                MainNumbersRange = 50
            });

        // Act
        var result = await sut.GetActiveConfigurationAsync(lotteryId: 7);

        // Assert
        result!.Id.Should().Be(2);
    }

    [Test]
    public async Task Given_Null_Bonus_Values_When_GetActiveConfigurationAsync_Is_Invoked_Should_Default_BonusNumbersCount_To_Zero()
    {
        // Arrange
        var (sut, options) = CreateSut();
        await Seed(options, new LotteryConfiguration
        {
            Id = 1,
            LotteryId = 7,
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = null,
            MainNumbersCount = 5,
            MainNumbersRange = 50,
            BonusNumbersCount = null,
            BonusNumbersRange = null
        });

        // Act
        var result = await sut.GetActiveConfigurationAsync(lotteryId: 7);

        // Assert
        result!.BonusNumbersCount.Should().Be(0);
    }

    private static (LotteryConfigurationRepository sut, DbContextOptions<LotteryDbContext> options) CreateSut()
    {
        var dbName = $"LotteryConfigRepoTests-{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<LotteryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factory = Substitute.For<IDbContextFactory<LotteryDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
               .Returns(_ => new LotteryDbContext(options));

        return (new LotteryConfigurationRepository(factory), options);
    }

    private static async Task Seed(DbContextOptions<LotteryDbContext> options, params LotteryConfiguration[] configs)
    {
        await using var context = new LotteryDbContext(options);
        context.LotteryConfigurations.AddRange(configs);
        await context.SaveChangesAsync();
    }
}