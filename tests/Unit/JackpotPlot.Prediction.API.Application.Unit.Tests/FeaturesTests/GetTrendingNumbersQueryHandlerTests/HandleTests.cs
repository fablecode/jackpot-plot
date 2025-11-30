using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Prediction.API.Application.Features.GetTrendingNumbers;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.FeaturesTests.GetTrendingNumbersQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private IPredictionRepository _predictionRepository = null!;
    private GetTrendingNumbersQueryHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _predictionRepository = Substitute.For<IPredictionRepository>();
        _sut = new GetTrendingNumbersQueryHandler(_predictionRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_Repository_GetTrendingNumbers()
    {
        // Arrange
        var trending = new Dictionary<int, int> { { 4, 10 } };
        _predictionRepository.GetTrendingNumbers().Returns(trending);

        var query = new GetTrendingNumbersQuery();

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _predictionRepository.Received(1).GetTrendingNumbers();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Success()
    {
        // Arrange
        var trending = new Dictionary<int, int> { { 2, 5 } };
        _predictionRepository.GetTrendingNumbers().Returns(trending);

        var query = new GetTrendingNumbersQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Trending_Data()
    {
        // Arrange
        var expected = new Dictionary<int, int>
            {
                { 1, 7 },
                { 9, 3 }
            };

        _predictionRepository.GetTrendingNumbers().Returns(expected);

        var query = new GetTrendingNumbersQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expected);
    }
}