using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Infrastructure.Databases;
using JackpotPlot.Lottery.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using FluentAssertions;

namespace JackpotPlot.Lottery.API.Infrastructure.Unit.Tests.RepositoriesTests.DrawResultRepositoryTests;

[TestFixture]
public class AddTests
{
    [Test]
    public async Task Given_Draw_When_Add_Is_Invoked_Should_Persist_DrawId()
    {
        // Arrange
        var (sut, options) = CreateSut();
        const int drawId = 123;

        var draw = new EurojackpotResult
        {
            MainNumbers = [1, 2, 3, 4, 5],
            EuroNumbers = [6, 7]
        };

        // Act
        await sut.Add(drawId, draw);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.DrawResults.Single().DrawId.Should().Be(drawId);
    }

    [Test]
    public async Task Given_Draw_When_Add_Is_Invoked_Should_Persist_MainNumbers_As_Numbers()
    {
        // Arrange
        var (sut, options) = CreateSut();
        const int drawId = 123;

        var draw = new EurojackpotResult
        {
            MainNumbers = [10, 20, 30],
            EuroNumbers = [1, 2]
        };

        // Act
        await sut.Add(drawId, draw);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.DrawResults.Single().Numbers.Should().Equal(draw.MainNumbers);
    }

    [Test]
    public async Task Given_Draw_When_Add_Is_Invoked_Should_Persist_EuroNumbers_As_BonusNumbers()
    {
        // Arrange
        var (sut, options) = CreateSut();
        const int drawId = 123;

        var draw = new EurojackpotResult
        {
            MainNumbers = [1, 2, 3, 4, 5],
            EuroNumbers = [9, 10]
        };

        // Act
        await sut.Add(drawId, draw);

        // Assert
        await using var verify = new LotteryDbContext(options);
        verify.DrawResults.Single().BonusNumbers.Should().Equal(draw.EuroNumbers);
    }

    [Test]
    public async Task Given_Draw_When_Add_Is_Invoked_Should_Return_New_DrawResult_Id()
    {
        // Arrange
        var (sut, _) = CreateSut();
        const int drawId = 123;

        var draw = new EurojackpotResult
        {
            MainNumbers = [1, 2, 3, 4, 5],
            EuroNumbers = [6, 7]
        };

        // Act
        var id = await sut.Add(drawId, draw);

        // Assert
        id.Should().BeGreaterThan(0);
    }

    private static (DrawResultRepository sut, DbContextOptions<LotteryDbContext> options) CreateSut()
    {
        var dbName = $"DrawResultRepoTests-{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<LotteryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factory = Substitute.For<IDbContextFactory<LotteryDbContext>>();

        factory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => new LotteryDbContext(options));

        return (new DrawResultRepository(factory), options);
    }
}