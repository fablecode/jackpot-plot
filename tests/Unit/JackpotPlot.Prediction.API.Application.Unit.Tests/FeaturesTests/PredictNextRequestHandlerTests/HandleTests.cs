using FluentAssertions;
using JackpotPlot.Application.Abstractions.Common;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions;
using JackpotPlot.Domain.ValueObjects;
using JackpotPlot.Prediction.API.Application.Features.PredictNext;
using JackpotPlot.Primitives.Algorithms;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace JackpotPlot.Prediction.API.Application.Unit.Tests.FeaturesTests.PredictNextRequestHandlerTests;

[TestFixture]
public class HandleTests
{
    private ILotteryConfigurationRepository _config = null!;
    private ILotteryHistoryRepository _history = null!;
    private IPredictionRepository _predictions = null!;
    private ILotteryStatisticsRepository _stats = null!;
    private IRandomProvider _random = null!;
    private IPredictionAlgorithm _algorithm = null!;
    private IServiceProvider _serviceProvider = null!;

    private PredictNextRequestHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _config = Substitute.For<ILotteryConfigurationRepository>();
        _history = Substitute.For<ILotteryHistoryRepository>();
        _predictions = Substitute.For<IPredictionRepository>();
        _stats = Substitute.For<ILotteryStatisticsRepository>();
        _random = Substitute.For<IRandomProvider>();
        _algorithm = Substitute.For<IPredictionAlgorithm>();

        // Real DI container so keyed services work
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IPredictionAlgorithm>(
            PredictionAlgorithmKeys.Random,
            (_, _) => _algorithm);

        _serviceProvider = services.BuildServiceProvider();

        _sut = new PredictNextRequestHandler(
            _config,
            _history,
            _predictions,
            _stats,
            _random,
            _serviceProvider);
    }

    [Test]
    public async Task Given_No_Config_When_Handle_Is_Invoked_Should_Return_Failure()
    {
        // Arrange
        _config.GetActiveConfigurationAsync(1).Returns((LotteryConfigurationDomain?)null);

        var request = new PredictNextRequest(1);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task Given_No_History_When_Handle_Is_Invoked_Should_Return_Failure()
    {
        // Arrange
        var cfg = CreateConfig(5);

        _config.GetActiveConfigurationAsync(5).Returns(cfg);
        _history.GetHistoricalDraws(5).Returns(new List<HistoricalDraw>());

        var request = new PredictNextRequest(5);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Invoke_Algorithm_Predict()
    {
        // Arrange
        var cfg = CreateConfig();
        var draws = new List<HistoricalDraw> { CreateDraw(1, 1, 2, 3) };

        _config.GetActiveConfigurationAsync(1).Returns(cfg);
        _history.GetHistoricalDraws(1).Returns(draws);
        _random.Get().Returns(new Random(1));

        var predicted = CreatePrediction(1, 2, 3);
        _algorithm
            .Predict(cfg, Arg.Any<ReadOnlyCollection<HistoricalDraw>>(), Arg.Any<Random>())
            .Returns(predicted);

        _stats.GetHotColdNumbers(0, null!, TimeSpan.Zero, null!)
            .ReturnsForAnyArgs(ImmutableArray<NumberStatus>.Empty);

        var request = new PredictNextRequest(1, 1);

        // Act
        _ = await _sut.Handle(request, CancellationToken.None);

        // Assert
        _algorithm.Received(1).Predict(
            cfg,
            Arg.Any<ReadOnlyCollection<HistoricalDraw>>(),
            Arg.Any<Random>());
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Store_Prediction()
    {
        // Arrange
        var cfg = CreateConfig();
        var draws = new List<HistoricalDraw> { CreateDraw(1, 1, 2, 3) };

        _config.GetActiveConfigurationAsync(1).Returns(cfg);
        _history.GetHistoricalDraws(1).Returns(draws);
        _random.Get().Returns(new Random(2));

        var predicted = CreatePrediction(4, 5, 6);
        _algorithm
            .Predict(cfg, Arg.Any<ReadOnlyCollection<HistoricalDraw>>(), Arg.Any<Random>())
            .Returns(predicted);

        _stats.GetHotColdNumbers(0, null!, TimeSpan.Zero, null!)
            .ReturnsForAnyArgs(ImmutableArray<NumberStatus>.Empty);

        var request = new PredictNextRequest(1, 1);

        // Act
        _ = await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _predictions.Received(1).Add(request.UserId, predicted);
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Retrieve_Stats_For_Main()
    {
        // Arrange
        var cfg = CreateConfig();
        var draws = new List<HistoricalDraw> { CreateDraw(1, 1, 2) };

        _config.GetActiveConfigurationAsync(1).Returns(cfg);
        _history.GetHistoricalDraws(1).Returns(draws);
        _random.Get().Returns(new Random(3));

        var predicted = CreatePrediction(1, 2);
        _algorithm
            .Predict(cfg, Arg.Any<ReadOnlyCollection<HistoricalDraw>>(), Arg.Any<Random>())
            .Returns(predicted);

        _stats.GetHotColdNumbers(0, null!, TimeSpan.Zero, null!)
            .ReturnsForAnyArgs(ImmutableArray<NumberStatus>.Empty);

        var request = new PredictNextRequest(1, 1);

        // Act
        _ = await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _stats.Received(1)
            .GetHotColdNumbers(1, Arg.Any<List<int>>(), TimeSpan.Zero, "main");
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Return_PlayOutput_Count_Matching_Request()
    {
        // Arrange
        var cfg = CreateConfig();
        var draws = new List<HistoricalDraw> { CreateDraw(1, 1) };

        _config.GetActiveConfigurationAsync(1).Returns(cfg);
        _history.GetHistoricalDraws(1).Returns(draws);
        _random.Get().Returns(new Random(4));

        var predicted = CreatePrediction(1);
        _algorithm
            .Predict(cfg, Arg.Any<ReadOnlyCollection<HistoricalDraw>>(), Arg.Any<Random>())
            .Returns(predicted);

        _stats.GetHotColdNumbers(0, null!, TimeSpan.Zero, null!)
              .ReturnsForAnyArgs(ImmutableArray<NumberStatus>.Empty);

        var request = new PredictNextRequest(1, 3);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Value.Plays.Length.Should().Be(3);
    }

    // --------------------------------------------------------------------
    // Helpers
    // --------------------------------------------------------------------

    private static LotteryConfigurationDomain CreateConfig(int lotteryId = 1)
        => new()
        {
            LotteryId = lotteryId,
            MainNumbersCount = 3,
            MainNumbersRange = 50,
            BonusNumbersCount = 1,
            BonusNumbersRange = 10
        };

    private static HistoricalDraw CreateDraw(
        int lotteryId = 1,
        params int[] main)
        => new(
            DrawId: 1,
            LotteryId: lotteryId,
            DrawDate: DateTime.UtcNow,
            WinningNumbers: new List<int>(main),
            BonusNumbers: new List<int>(),
            CreatedAt: DateTime.UtcNow);

    private static PredictionResult CreatePrediction(params int[] main)
        => new(
            LotteryId: 1,
            AlgorithmKey: PredictionAlgorithmKeys.Random,
            PredictedNumbers: main.ToImmutableArray(),
            BonusNumbers: ImmutableArray<int>.Empty,
            ConfidenceScore: 0.5d);
}