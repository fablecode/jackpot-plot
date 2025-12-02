using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Application.Features.GetLotteries;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.GetLotteriesQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private ILotteryRepository _lotteryRepository = null!;
    private GetLotteriesQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _lotteryRepository = Substitute.For<ILotteryRepository>();
        _sut = new GetLotteriesQueryHandler(_lotteryRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_Repository_GetLotteries()
    {
        // Arrange
        var lotteries = new List<LotteryDomain>();
        _lotteryRepository.GetLotteries().Returns(Task.FromResult<ICollection<LotteryDomain>>(lotteries));

        var query = new GetLotteriesQuery();

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _lotteryRepository.Received(1).GetLotteries();
    }

    [Test]
    public async Task Given_Repository_Result_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var lotteries = new List<LotteryDomain> { new() { Id = 1, Name = "Test" } };

        _lotteryRepository.GetLotteries().Returns(Task.FromResult<ICollection<LotteryDomain>>(lotteries));

        var query = new GetLotteriesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Result_When_Handle_Is_Invoked_Should_Return_Lotteries_From_Repository()
    {
        // Arrange
        var lotteries = new List<LotteryDomain>
            {
                new() { Id = 1, Name = "Lotto A" },
                new() { Id = 2, Name = "Lotto B" }
            };

        _lotteryRepository.GetLotteries().Returns(Task.FromResult<ICollection<LotteryDomain>>(lotteries));

        var query = new GetLotteriesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(lotteries);
    }
}