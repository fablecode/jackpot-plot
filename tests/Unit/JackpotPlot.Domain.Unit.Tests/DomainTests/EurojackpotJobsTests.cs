using FluentAssertions;
using JackpotPlot.Domain.Constants;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace JackpotPlot.Domain.Unit.Tests.DomainTests;

[TestFixture]
public class EurojackpotJobsTests
{
    // Utility: IEnumerable -> IAsyncEnumerable (kept small and obvious)
    private static IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> items) => items.ToAsyncEnumerable();

    [Test]
    public async Task Given_3_Results_When_FetchDrawHistory_Is_Invoked_Then_It_Publishes_Exactly_3_Times()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotJobs>>();
        var results = new[] { new EurojackpotResult(), new EurojackpotResult(), new EurojackpotResult() };

        var eurojackpotService = Substitute.For<IEurojackpotService>();
        eurojackpotService.GetAllDrawHistoryResultsAsync()
                          .Returns(ToAsyncEnumerable(results));

        var queueWriter = Substitute.For<IQueueWriter<Message<EurojackpotResult>>>();
        var sut = new EurojackpotJobs(logger, eurojackpotService, queueWriter);

        var expectedRoutingKey = string.Join('.', RoutingKeys.LotteryResults, EventTypes.EurojackpotDraw);

        // Act
        await sut.FetchDrawHistory();

        // Assert (one observable)
        _ = queueWriter.Received(results.Length)
                   .Publish(Arg.Any<Message<EurojackpotResult>>(), expectedRoutingKey);
    }

    [Test]
    public async Task Given_3_Results_When_Is_Invoked_FetchDrawHistory_Then_All_RoutingKeys_Are_Correct()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotJobs>>();
        var results = new[] { new EurojackpotResult(), new EurojackpotResult(), new EurojackpotResult() };

        var eurojackpotService = Substitute.For<IEurojackpotService>();
        eurojackpotService.GetAllDrawHistoryResultsAsync()
                          .Returns(ToAsyncEnumerable(results));

        var queueWriter = Substitute.For<IQueueWriter<Message<EurojackpotResult>>>();
        var published = new List<(Message<EurojackpotResult> msg, string rk)>();
        queueWriter.Publish(Arg.Any<Message<EurojackpotResult>>(), Arg.Any<string>())
                   .Returns(ci =>
                   {
                       published.Add((ci.Arg<Message<EurojackpotResult>>(), ci.Arg<string>()));
                       return Task.CompletedTask;
                   });

        var sut = new EurojackpotJobs(logger, eurojackpotService, queueWriter);
        var expectedRoutingKey = string.Join('.', RoutingKeys.LotteryResults, EventTypes.EurojackpotDraw);

        // Act
        await sut.FetchDrawHistory();

        // Assert (one observable)
        published.Select(p => p.rk).Should().OnlyContain(rk => rk == expectedRoutingKey);
    }

    [Test]
    public async Task Given_3_Results_When_FetchDrawHistory_Is_Invoked_Then_All_Payloads_Match_Source_Results()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotJobs>>();
        var r1 = new EurojackpotResult();
        var r2 = new EurojackpotResult();
        var r3 = new EurojackpotResult();
        var results = new[] { r1, r2, r3 };

        var eurojackpotService = Substitute.For<IEurojackpotService>();
        eurojackpotService.GetAllDrawHistoryResultsAsync()
                          .Returns(ToAsyncEnumerable(results));

        var queueWriter = Substitute.For<IQueueWriter<Message<EurojackpotResult>>>();
        var published = new List<Message<EurojackpotResult>>();
        queueWriter.Publish(Arg.Any<Message<EurojackpotResult>>(), Arg.Any<string>())
                   .Returns(ci =>
                   {
                       published.Add(ci.Arg<Message<EurojackpotResult>>());
                       return Task.CompletedTask;
                   });

        var sut = new EurojackpotJobs(logger, eurojackpotService, queueWriter);

        // Act
        await sut.FetchDrawHistory();

        // Assert (one observable)
        published.Select(m => m.Data).Should().BeEquivalentTo(results);
    }

    [Test]
    public async Task Given_3_Results_When_FetchDrawHistory_Is_Invoked_Then_It_Logs_Once_Per_Publish()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotJobs>>();
        var results = new[] { new EurojackpotResult(), new EurojackpotResult(), new EurojackpotResult() };

        var eurojackpotService = Substitute.For<IEurojackpotService>();
        eurojackpotService.GetAllDrawHistoryResultsAsync()
                          .Returns(ToAsyncEnumerable(results));

        var queueWriter = Substitute.For<IQueueWriter<Message<EurojackpotResult>>>();
        var sut = new EurojackpotJobs(logger, eurojackpotService, queueWriter);

        // Act
        await sut.FetchDrawHistory();

        // Assert (one observable)
        logger.ShouldHaveLoggedInformationContaining("Publishing Eurojackpot draw result for", times: results.Length);
    }

    [Test]
    public async Task Given_An_Empty_History_When_FetchDrawHistory_Is_Invoked_Then_It_Does_Not_Publish()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotJobs>>();
        var eurojackpotService = Substitute.For<IEurojackpotService>();
        eurojackpotService.GetAllDrawHistoryResultsAsync()
                          .Returns(ToAsyncEnumerable(Enumerable.Empty<EurojackpotResult>()));

        var queueWriter = Substitute.For<IQueueWriter<Message<EurojackpotResult>>>();
        var sut = new EurojackpotJobs(logger, eurojackpotService, queueWriter);

        // Act
        await sut.FetchDrawHistory();

        // Assert (one observable)
        await queueWriter.DidNotReceive()
                         .Publish(Arg.Any<Message<EurojackpotResult>>(), Arg.Any<string>());
    }

    [Test]
    public async Task Given_Empty_History_When_FetchDrawHistory_Then_Does_Not_Log_Publish_Attempts()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotJobs>>();
        var eurojackpotService = Substitute.For<IEurojackpotService>();
        eurojackpotService.GetAllDrawHistoryResultsAsync()
                          .Returns(ToAsyncEnumerable(Enumerable.Empty<EurojackpotResult>()));

        var queueWriter = Substitute.For<IQueueWriter<Message<EurojackpotResult>>>();
        var sut = new EurojackpotJobs(logger, eurojackpotService, queueWriter);

        // Act
        await sut.FetchDrawHistory();

        // Assert (one observable)
        logger.ShouldHaveLoggedInformationContaining("Publishing Eurojackpot draw result for", times: 0);
    }
}

// ---------- Minimal utilities kept for clarity ----------
internal static class TestExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
        this IEnumerable<T> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
            await Task.Yield();
        }
    }

    public static void ShouldHaveLoggedInformationContaining<T>(
        this ILogger<T> logger,
        string contains,
        int? times = null)
    {
        var calls = logger.ReceivedCalls()
                          .Where(c => c.GetMethodInfo().Name == nameof(ILogger.Log));

        bool IsInfoWithMatch(object?[] args)
        {
            if (args.Length < 5) return false;
            var level = (LogLevel)args[0]!;
            var state = args[2];
            return level == LogLevel.Information && (state?.ToString()?.Contains(contains) ?? false);
        }

        var count = calls.Count(c => IsInfoWithMatch(c.GetArguments()));
        if (times.HasValue)
            count.Should().Be(times.Value, $"expected {times} Information logs containing '{contains}'");
        else
            count.Should().BeGreaterThan(0, $"expected at least one Information log containing '{contains}'");
    }
}