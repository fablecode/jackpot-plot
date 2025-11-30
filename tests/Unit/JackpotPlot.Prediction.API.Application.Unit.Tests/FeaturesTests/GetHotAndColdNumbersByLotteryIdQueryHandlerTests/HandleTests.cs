using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbersByLotteryId;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.FeaturesTests.GetHotAndColdNumbersByLotteryIdQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private IPredictionRepository _predictionRepository = null!;
    private GetHotAndColdNumbersByLotteryIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _predictionRepository = Substitute.For<IPredictionRepository>();
        _sut = new GetHotAndColdNumbersByLotteryIdQueryHandler(_predictionRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_Repository_With_Correct_LotteryId()
    {
        // Arrange
        var hot = new Dictionary<int, int> { { 1, 10 } };
        var cold = new Dictionary<int, int> { { 2, 5 } };

        _predictionRepository
            .GetHotColdNumbersByLotteryId(123)
            .Returns(Task.FromResult((hot, cold)));

        var query = new GetHotAndColdNumbersByLotteryIdQuery(123);

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _predictionRepository.Received(1).GetHotColdNumbersByLotteryId(123);
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var hot = new Dictionary<int, int> { { 5, 20 } };
        var cold = new Dictionary<int, int> { { 9, 1 } };

        _predictionRepository
            .GetHotColdNumbersByLotteryId(7)
            .Returns(Task.FromResult((hot, cold)));

        var query = new GetHotAndColdNumbersByLotteryIdQuery(7);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Map_HotNumbers_Correctly()
    {
        // Arrange
        var hot = new Dictionary<int, int> { { 3, 15 }, { 8, 7 } };
        var cold = new Dictionary<int, int> { { 6, 2 } };

        _predictionRepository
            .GetHotColdNumbersByLotteryId(100)
            .Returns(Task.FromResult((hot, cold)));

        var query = new GetHotAndColdNumbersByLotteryIdQuery(100);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.HotNumbers.Should().BeEquivalentTo(hot);
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Map_ColdNumbers_Correctly()
    {
        // Arrange
        var hot = new Dictionary<int, int> { { 1, 3 } };
        var cold = new Dictionary<int, int> { { 7, 1 }, { 9, 4 } };

        _predictionRepository
            .GetHotColdNumbersByLotteryId(22)
            .Returns(Task.FromResult((hot, cold)));

        var query = new GetHotAndColdNumbersByLotteryIdQuery(22);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ColdNumbers.Should().BeEquivalentTo(cold);
    }
}