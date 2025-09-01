using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Domain.Messaging;
using JackpotPlot.Domain.Messaging.Processors;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Domain.Unit.Tests.MessagingTests.ProcessorsTests;

[TestFixture]
public class EurojackpotResultMessageProcessorTests
{
    // ---------------- Helpers kept tiny & obvious ----------------
    private static EurojackpotResult CreateResult(
        DateTime? date = null,
        IEnumerable<int>? main = null,
        IEnumerable<int>? euro = null)
    {
        return new EurojackpotResult
        {
            Date = date ?? new DateTime(2024, 1, 5),
            MainNumbers = (main ?? new[] { 1, 2, 3, 4, 5 }).ToImmutableArray(),
            EuroNumbers = (euro ?? new[] { 6, 7 }).ToImmutableArray(),
        };
    }

    private static Message<EurojackpotResult> Wrap(EurojackpotResult r)
        => new Message<EurojackpotResult>("Eurojackpot.Draw", r);

    // ---------------- Success path: draw does NOT exist ----------------

    [Test]
    public async Task Given_New_Draw_When_ProcessAsync_Method_Is_Invoked_Then_Add_Method_Is_Called_Once()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotResultMessageProcessor>>();
        var lotteryRepo = Substitute.For<ILotteryRepository>();
        var drawRepo = Substitute.For<IDrawRepository>();

        lotteryRepo.GetLotteryIdByName("Eurojackpot").Returns(42);
        drawRepo.DrawExist(42, Arg.Any<DateTime>(), Arg.Any<ImmutableArray<int>>(), Arg.Any<ImmutableArray<int>>())
                .Returns(Task.FromResult(false));

        var sut = new EurojackpotResultMessageProcessor(logger, lotteryRepo, drawRepo);
        var result = CreateResult();
        var message = Wrap(result);

        // Act
        await sut.ProcessAsync(message, CancellationToken.None);

        // Assert (one observable)
        await drawRepo.Received(1).Add(42, result);
    }

    [Test]
    public async Task Given_New_Draw_When_ProcessAsync_Method_Is_invoked_Then_DrawExist_Method_Is_Queried_With_Correct_Arguments()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotResultMessageProcessor>>();
        var lotteryRepo = Substitute.For<ILotteryRepository>();
        var drawRepo = Substitute.For<IDrawRepository>();

        lotteryRepo.GetLotteryIdByName("Eurojackpot").Returns(7);

        var main = new[] { 5, 8, 13, 21, 34 }.ToImmutableArray();
        var euro = new[] { 3, 9 }.ToImmutableArray();
        var dto = CreateResult(new DateTime(2024, 2, 2), main, euro);
        var message = Wrap(dto);

        drawRepo.DrawExist(7, Arg.Any<DateTime>(), Arg.Any<ImmutableArray<int>>(), Arg.Any<ImmutableArray<int>>())
                .Returns(Task.FromResult(false));

        var sut = new EurojackpotResultMessageProcessor(logger, lotteryRepo, drawRepo);

        // Act
        await sut.ProcessAsync(message, CancellationToken.None);

        // Assert (one observable)
        await drawRepo.Received(1).DrawExist(
            7,
            dto.Date,
            Arg.Is(main),
            Arg.Is(euro));
    }

    // ---------------- Failure path: draw already exists ----------------

    [Test]
    public async Task Given_Existing_Draw_When_ProcessAsync_Method_Is_Invoked_Then_Add_Is_Not_Called()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotResultMessageProcessor>>();
        var lotteryRepo = Substitute.For<ILotteryRepository>();
        var drawRepo = Substitute.For<IDrawRepository>();

        lotteryRepo.GetLotteryIdByName("Eurojackpot").Returns(42);
        drawRepo.DrawExist(42, Arg.Any<DateTime>(), Arg.Any<ImmutableArray<int>>(), Arg.Any<ImmutableArray<int>>())
                .Returns(Task.FromResult(true));

        var sut = new EurojackpotResultMessageProcessor(logger, lotteryRepo, drawRepo);
        var dto = CreateResult();
        var message = Wrap(dto);

        // Act
        await sut.ProcessAsync(message, CancellationToken.None);

        // Assert (one observable)
        await drawRepo.DidNotReceive().Add(Arg.Any<int>(), Arg.Any<EurojackpotResult>());
    }

    // ---------------- Caching: lottery id looked up once ----------------

    [Test]
    public async Task Given_Multiple_Calls_To_ProcessAsync_Method_Then_LotteryId_Is_Looked_Up_Once()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotResultMessageProcessor>>();
        var lotteryRepo = Substitute.For<ILotteryRepository>();
        var drawRepo = Substitute.For<IDrawRepository>();

        lotteryRepo.GetLotteryIdByName("Eurojackpot").Returns(99);
        drawRepo.DrawExist(99, Arg.Any<DateTime>(), Arg.Any<ImmutableArray<int>>(), Arg.Any<ImmutableArray<int>>())
                .Returns(Task.FromResult(false));

        var sut = new EurojackpotResultMessageProcessor(logger, lotteryRepo, drawRepo);

        var m1 = Wrap(CreateResult(new DateTime(2024, 1, 5)));
        var m2 = Wrap(CreateResult(new DateTime(2024, 1, 12)));

        // Act
        await sut.ProcessAsync(m1, CancellationToken.None);
        await sut.ProcessAsync(m2, CancellationToken.None);

        // Assert (one observable)
        await lotteryRepo.Received(1).GetLotteryIdByName("Eurojackpot");
    }

    [Test]
    public async Task Given_New_Draw_When_ProcessAsync_Method_Is_Invoked_Then_Result_Is_Success()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotResultMessageProcessor>>();
        var lotteryRepo = Substitute.For<ILotteryRepository>();
        var drawRepo = Substitute.For<IDrawRepository>();
        lotteryRepo.GetLotteryIdByName("Eurojackpot").Returns(1);
        drawRepo.DrawExist(1, Arg.Any<DateTime>(), Arg.Any<ImmutableArray<int>>(), Arg.Any<ImmutableArray<int>>())
                .Returns(Task.FromResult(false));

        var sut = new EurojackpotResultMessageProcessor(logger, lotteryRepo, drawRepo);
        var message = Wrap(CreateResult());

        // Act
        var result = await sut.ProcessAsync(message, CancellationToken.None);

        // Assert (one observable) — adjust property name if needed
        result.GetType().GetProperty("IsSuccess")!.GetValue(result).Should().Be(true);
    }

    [Test]
    public async Task Given_Existing_Draw_When_ProcessAsync_Method_Is_invoked_Then_Result_Is_Failure()
    {
        // Arrange
        var logger = Substitute.For<ILogger<EurojackpotResultMessageProcessor>>();
        var lotteryRepo = Substitute.For<ILotteryRepository>();
        var drawRepo = Substitute.For<IDrawRepository>();
        lotteryRepo.GetLotteryIdByName("Eurojackpot").Returns(1);
        drawRepo.DrawExist(1, Arg.Any<DateTime>(), Arg.Any<ImmutableArray<int>>(), Arg.Any<ImmutableArray<int>>())
                .Returns(Task.FromResult(true));

        var sut = new EurojackpotResultMessageProcessor(logger, lotteryRepo, drawRepo);
        var message = Wrap(CreateResult());

        // Act
        var result = await sut.ProcessAsync(message, CancellationToken.None);

        // Assert (one observable) — adjust property name if needed
        result.GetType().GetProperty("IsSuccess")!.GetValue(result).Should().Be(false);
    }
}