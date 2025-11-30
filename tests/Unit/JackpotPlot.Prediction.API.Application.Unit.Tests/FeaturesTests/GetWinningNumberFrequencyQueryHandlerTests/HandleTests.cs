using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using JackpotPlot.Prediction.API.Application.Features.GetWinningNumberFrequency;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.FeaturesTests.GetWinningNumberFrequencyQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private ILotteryHistoryRepository _lotteryHistoryRepository = null!;
    private GetWinningNumberFrequencyQueryHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _lotteryHistoryRepository = Substitute.For<ILotteryHistoryRepository>();
        _sut = new GetWinningNumberFrequencyQueryHandler(_lotteryHistoryRepository);
    }

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_GetWinningNumberFrequency_On_Repository()
    {
        // Arrange
        var freq = ImmutableArray.Create(
            new WinningNumberFrequencyResult
            {
                Number = 1,
                FrequencyOverTime = new Dictionary<string, int> { { "Last10", 5 } }
            }
        );
        _lotteryHistoryRepository.GetWinningNumberFrequency().Returns(freq);

        var query = new GetWinningNumberFrequencyQuery();

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _lotteryHistoryRepository.Received(1).GetWinningNumberFrequency();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Success()
    {
        // Arrange
        var freq = ImmutableArray.Create(
            new WinningNumberFrequencyResult
            {
                Number = 2,
                FrequencyOverTime = new Dictionary<string, int> { { "Last30", 12 } }
            }
        );
        _lotteryHistoryRepository.GetWinningNumberFrequency().Returns(freq);

        var query = new GetWinningNumberFrequencyQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Response_When_Handle_Is_Invoked_Should_Return_Exact_Frequency_Data()
    {
        // Arrange
        var expected = ImmutableArray.Create(
            new WinningNumberFrequencyResult
            {
                Number = 3,
                FrequencyOverTime = new Dictionary<string, int>
                {
                        { "Last10", 4 },
                        { "Last30", 9 }
                }
            },
            new WinningNumberFrequencyResult
            {
                Number = 7,
                FrequencyOverTime = new Dictionary<string, int>
                {
                        { "Last10", 2 },
                        { "Last30", 6 }
                }
            }
        );

        _lotteryHistoryRepository.GetWinningNumberFrequency().Returns(expected);

        var query = new GetWinningNumberFrequencyQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expected);
    }
}