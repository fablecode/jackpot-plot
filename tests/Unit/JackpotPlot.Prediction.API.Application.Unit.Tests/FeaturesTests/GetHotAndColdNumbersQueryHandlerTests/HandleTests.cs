using NSubstitute;
using NUnit.Framework;
using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbers;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.FeaturesTests.GetHotAndColdNumbersQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private ILotteryHistoryRepository _lotteryHistoryRepository = null!;
    private GetHotAndColdNumbersQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _lotteryHistoryRepository = Substitute.For<ILotteryHistoryRepository>();
        _sut = new GetHotAndColdNumbersQueryHandler(_lotteryHistoryRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_Repository_GetAll_Once()
    {
        // Arrange
        var hot = new Dictionary<int, int> { { 1, 10 } };
        var cold = new Dictionary<int, int> { { 2, 1 } };
        _lotteryHistoryRepository
            .GetAll()
            .Returns(Task.FromResult((hot, cold)));

        var query = new GetHotAndColdNumbersQuery();
        var ct = CancellationToken.None;

        // Act
        _ = await _sut.Handle(query, ct);

        // Assert
        await _lotteryHistoryRepository.Received(1).GetAll();
    }

    [Test]
    public async Task Given_Repository_Result_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var hot = new Dictionary<int, int> { { 1, 10 } };
        var cold = new Dictionary<int, int> { { 2, 1 } };
        _lotteryHistoryRepository
            .GetAll()
            .Returns(Task.FromResult((hot, cold)));

        var query = new GetHotAndColdNumbersQuery();
        var ct = CancellationToken.None;

        // Act
        var result = await _sut.Handle(query, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Result_When_Handle_Is_Invoked_Should_Map_HotNumbers_To_Output()
    {
        // Arrange
        var hot = new Dictionary<int, int> { { 1, 10 }, { 3, 5 } };
        var cold = new Dictionary<int, int> { { 2, 1 } };
        _lotteryHistoryRepository
            .GetAll()
            .Returns(Task.FromResult((hot, cold)));

        var query = new GetHotAndColdNumbersQuery();
        var ct = CancellationToken.None;

        // Act
        var result = await _sut.Handle(query, ct);

        // Assert
        result.Value.HotNumbers.Should().BeEquivalentTo(hot);
    }

    [Test]
    public async Task Given_Repository_Result_When_Handle_Is_Invoked_Should_Map_ColdNumbers_To_Output()
    {
        // Arrange
        var hot = new Dictionary<int, int> { { 1, 10 } };
        var cold = new Dictionary<int, int> { { 2, 1 }, { 4, 2 } };
        _lotteryHistoryRepository
            .GetAll()
            .Returns(Task.FromResult((hot, cold)));

        var query = new GetHotAndColdNumbersQuery();
        var ct = CancellationToken.None;

        // Act
        var result = await _sut.Handle(query, ct);

        // Assert
        result.Value.ColdNumbers.Should().BeEquivalentTo(cold);
    }
}