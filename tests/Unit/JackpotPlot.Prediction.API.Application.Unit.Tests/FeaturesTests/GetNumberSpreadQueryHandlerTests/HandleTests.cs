using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.GetNumberSpread;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.FeaturesTests.GetNumberSpreadQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private IPredictionRepository _predictionRepository = null!;
    private GetNumberSpreadQueryHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _predictionRepository = Substitute.For<IPredictionRepository>();
        _sut = new GetNumberSpreadQueryHandler(_predictionRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_Repository_GetNumberSpread()
    {
        // Arrange
        var spread = new NumberSpreadResult(1, 5, 10);
        _predictionRepository.GetNumberSpread().Returns(spread);

        var query = new GetNumberSpreadQuery();

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _predictionRepository.Received(1).GetNumberSpread();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Success()
    {
        // Arrange
        var spread = new NumberSpreadResult(2, 6, 12);
        _predictionRepository.GetNumberSpread().Returns(spread);

        var query = new GetNumberSpreadQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Exact_Spread_Data()
    {
        // Arrange
        var expected = new NumberSpreadResult(Low: 3, Mid: 15, High: 45);
        _predictionRepository.GetNumberSpread().Returns(expected);

        var query = new GetNumberSpreadQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expected);
    }
}