using NSubstitute;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using JackpotPlot.Application.Abstractions.Messaging;
using JackpotPlot.Application.Abstractions.Services;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Models;
using JackpotPlot.Infrastructure.Jobs.Eurojackpot.FetchEurojackpotDrawHistory;
using Microsoft.Extensions.Logging;

namespace JackpotPlot.Infrastructure.Unit.Tests.JobsTests.EurojackpotTests.FetchEurojackpotDrawHistoryTests;

[TestFixture]
public sealed class FetchEurojackpotDrawHistoryRequestHandlerTests
{
    private ILogger<FetchEurojackpotDrawHistoryRequestHandler> _logger = null!;
    private IEurojackpotService _eurojackpotService = null!;
    private IQueueWriter<Message<EurojackpotResult>> _queueWriter = null!;
    private FetchEurojackpotDrawHistoryRequestHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<FetchEurojackpotDrawHistoryRequestHandler>>();
        _eurojackpotService = Substitute.For<IEurojackpotService>();
        _queueWriter = Substitute.For<IQueueWriter<Message<EurojackpotResult>>>();

        _sut = new FetchEurojackpotDrawHistoryRequestHandler(_logger, _eurojackpotService, _queueWriter);
    }

    [Test]
    public async Task Given_MultipleResults_When_Handle_Method_Is_Invoked_Then_It_Should_Publish_Once_Per_Result()
    {
        // Arrange
        var results = new[]
        {
                new EurojackpotResult { /* set what you need */ Date = new DateTime(2024, 1, 5) },
                new EurojackpotResult { /* set what you need */ Date = new DateTime(2024, 1, 12) }
        };
        _eurojackpotService.GetAllDrawHistoryResultsAsync()
            .Returns(ReturnAsync(results));

        var request = new FetchEurojackpotDrawHistoryRequest();
        var ct = CancellationToken.None;

        // Act
        await _sut.Handle(request, ct);

        // Assert (one assert)
        await _queueWriter.Received(results.Length)
            .Publish(Arg.Any<Message<EurojackpotResult>>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_OneResult_When_Handle_Method_Is_Invoked_Then_It_Should_Use_Expected_RoutingKey()
    {
        // Arrange
        var results = new[]
        {
                new EurojackpotResult { Date = new DateTime(2024, 1, 5) }
        };
        _eurojackpotService.GetAllDrawHistoryResultsAsync()
            .Returns(ReturnAsync(results));

        var expectedRoutingKey = string.Join('.', RoutingKeys.LotteryResults, EventTypes.EurojackpotDraw);

        var request = new FetchEurojackpotDrawHistoryRequest();
        var ct = CancellationToken.None;

        // Act
        await _sut.Handle(request, ct);

        // Assert (one assert)
        await _queueWriter.Received(1)
            .Publish(Arg.Any<Message<EurojackpotResult>>(),
                     Arg.Is<string>(rk => rk == expectedRoutingKey),
                     Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_CancellationToken_When_Handle_Then_Should_PassCancellationTokenToPublish()
    {
        // Arrange
        var results = new[]
        {
                new EurojackpotResult { Date = new DateTime(2024, 1, 5) }
        };
        _eurojackpotService.GetAllDrawHistoryResultsAsync()
            .Returns(ReturnAsync(results));

        using var cts = new CancellationTokenSource();
        var request = new FetchEurojackpotDrawHistoryRequest();

        // Act
        await _sut.Handle(request, cts.Token);

        // Assert (one assert)
        await _queueWriter.Received(1)
            .Publish(Arg.Any<Message<EurojackpotResult>>(),
                     Arg.Any<string>(),
                     Arg.Is<CancellationToken>(t => t == cts.Token));
    }

    [Test]
    public async Task Given_NoResults_When_Handle_Then_Should_NotPublish()
    {
        // Arrange
        var results = Array.Empty<EurojackpotResult>();
        _eurojackpotService.GetAllDrawHistoryResultsAsync()
            .Returns(ReturnAsync(results));

        var request = new FetchEurojackpotDrawHistoryRequest();
        var ct = CancellationToken.None;

        // Act
        await _sut.Handle(request, ct);

        // Assert (one assert)
        await _queueWriter.DidNotReceive()
            .Publish(Arg.Any<Message<EurojackpotResult>>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_Call_When_Handle_Then_Should_CallServiceOnce()
    {
        // Arrange
        var results = new[]
        {
                new EurojackpotResult { Date = new DateTime(2024, 1, 5) }
        };
        _eurojackpotService.GetAllDrawHistoryResultsAsync()
            .Returns(ReturnAsync(results));

        var request = new FetchEurojackpotDrawHistoryRequest();
        var ct = CancellationToken.None;

        // Act
        await _sut.Handle(request, ct);

        // Assert (one assert)
        _ = _eurojackpotService.Received(1).GetAllDrawHistoryResultsAsync(ct);
    }

    // --- helpers ---

    /// <summary>
    /// Converts an enumerable into an IAsyncEnumerable without needing extra packages.
    /// </summary>
    private static async IAsyncEnumerable<EurojackpotResult> ReturnAsync(
        IEnumerable<EurojackpotResult> items,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
            await Task.Yield();
        }
    }
}