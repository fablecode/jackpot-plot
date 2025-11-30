using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.GetLuckyPair;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.FeaturesTests.GetLuckyPairQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private IPredictionRepository _predictionRepository = null!;
    private GetLuckyPairQueryHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _predictionRepository = Substitute.For<IPredictionRepository>();
        _sut = new GetLuckyPairQueryHandler(_predictionRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_GetLuckyPair_On_Repository()
    {
        // Arrange
        var sample = ImmutableArray.Create(new LuckyPairResult(1, 2, 5));
        _predictionRepository.GetLuckyPair().Returns(sample);

        var query = new GetLuckyPairQuery();

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _predictionRepository.Received(1).GetLuckyPair();
    }

    [Test]
    public async Task Given_Repository_Returns_Data_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var sample = ImmutableArray.Create(new LuckyPairResult(3, 7, 10));
        _predictionRepository.GetLuckyPair().Returns(sample);

        var query = new GetLuckyPairQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Returns_Data_When_Handle_Is_Invoked_Should_Return_Exact_Pair_Data()
    {
        // Arrange
        var expected = ImmutableArray.Create(
            new LuckyPairResult(1, 9, 4),
            new LuckyPairResult(2, 8, 3)
        );

        _predictionRepository.GetLuckyPair().Returns(expected);

        var query = new GetLuckyPairQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expected);
    }
}