using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Prediction.API.Application.Features.GetPredictionSuccessRate;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.FeaturesTests.GetPredictionSuccessRateQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private IPredictionRepository _predictionRepository = null!;
    private GetPredictionSuccessRateQueryHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _predictionRepository = Substitute.For<IPredictionRepository>();
        _sut = new GetPredictionSuccessRateQueryHandler(_predictionRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_GetPredictionSuccessRate_On_Repository()
    {
        // Arrange
        var sample = ImmutableDictionary<int, int>.Empty;
        _predictionRepository.GetPredictionSuccessRate().Returns(sample);

        var query = new GetPredictionSuccessRateQuery();

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _predictionRepository.Received(1).GetPredictionSuccessRate();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Success()
    {
        // Arrange
        var expected = ImmutableDictionary<int, int>.Empty;
        _predictionRepository.GetPredictionSuccessRate().Returns(expected);

        var query = new GetPredictionSuccessRateQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Exact_Dictionary()
    {
        // Arrange
        var expected = ImmutableDictionary.CreateRange(new[]
        {
                new KeyValuePair<int, int>(5, 12),
                new KeyValuePair<int, int>(10, 20)
            });

        _predictionRepository.GetPredictionSuccessRate().Returns(expected);

        var query = new GetPredictionSuccessRateQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expected);
    }
}