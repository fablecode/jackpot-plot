using NSubstitute;
using NUnit.Framework;
using System.Collections.Immutable;
using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Lottery.API.Application.Features.AddTicketPlays;
using JackpotPlot.Lottery.API.Application.Models.Input;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.AddTicketPlaysRequestHandlerTests;

[TestFixture]
public class HandleTests
{
    private ITicketPlayRepository _ticketPlayRepository = null!;
    private AddTicketPlaysRequestHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _ticketPlayRepository = Substitute.For<ITicketPlayRepository>();
        _sut = new AddTicketPlaysRequestHandler(_ticketPlayRepository);
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Call_Repository_Add_With_Mapped_Plays()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[]
        {
            new CreateTicketPlaysInput(0, [1, 2, 3]),
            new CreateTicketPlaysInput(1, [4, 5])
        };

        var repoResult = ImmutableArray.Create(Guid.NewGuid());

        _ticketPlayRepository
            .Add(ticketId, Arg.Any<ImmutableArray<(int lineIndex, List<int> numbers)>>())
            .Returns(Task.FromResult(repoResult));

        var request = new AddTicketPlaysRequest(ticketId, plays);

        // Act
        _ = await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _ticketPlayRepository.Received(1).Add(
            ticketId,
            Arg.Is<ImmutableArray<(int lineIndex, List<int> numbers)>>(
                coll =>
                    coll.Count() == 2 &&
                    coll.ElementAt(0).lineIndex == 0 &&
                    coll.ElementAt(0).numbers.SequenceEqual(new[] { 1, 2, 3 }) &&
                    coll.ElementAt(1).lineIndex == 1 &&
                    coll.ElementAt(1).numbers.SequenceEqual(new[] { 4, 5 })
            )
        );
    }

    [Test]
    public async Task Given_Repository_Returns_NonEmpty_Result_When_Handle_Is_Invoked_Should_Return_Success()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[]
        {
                new CreateTicketPlaysInput(0, [1, 2, 3])
            };

        var repoResult = ImmutableArray.Create(Guid.NewGuid(), Guid.NewGuid());

        _ticketPlayRepository
            .Add(ticketId, Arg.Any<ImmutableArray<(int lineIndex, List<int> numbers)>>())
            .Returns(Task.FromResult(repoResult));

        var request = new AddTicketPlaysRequest(ticketId, plays);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Returns_NonEmpty_Result_When_Handle_Is_Invoked_Should_Return_Same_PlayIds()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[]
        {
                new CreateTicketPlaysInput(0, [1, 2, 3])
            };

        var repoResult = ImmutableArray.Create(Guid.NewGuid(), Guid.NewGuid());

        _ticketPlayRepository
            .Add(ticketId, Arg.Any<ImmutableArray<(int lineIndex, List<int> numbers)>>())
            .Returns(Task.FromResult(repoResult));

        var request = new AddTicketPlaysRequest(ticketId, plays);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(repoResult);
    }

    [Test]
    public async Task Given_Repository_Returns_Empty_Result_When_Handle_Is_Invoked_Should_Return_Failure()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[]
        {
                new CreateTicketPlaysInput(0, [1, 2, 3])
            };

        _ticketPlayRepository
            .Add(ticketId, Arg.Any<ImmutableArray<(int lineIndex, List<int> numbers)>>())
            .Returns(Task.FromResult(ImmutableArray<Guid>.Empty));

        var request = new AddTicketPlaysRequest(ticketId, plays);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Returns_Empty_Result_When_Handle_Is_Invoked_Should_Contain_TicketId_In_Error_Message()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var plays = new[]
        {
                new CreateTicketPlaysInput(0, new[] { 1, 2, 3 })
            };

        _ticketPlayRepository
            .Add(ticketId, Arg.Any<ImmutableArray<(int lineIndex, List<int> numbers)>>())
            .Returns(Task.FromResult(ImmutableArray<Guid>.Empty));

        var request = new AddTicketPlaysRequest(ticketId, plays);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(error => error.Contains(ticketId.ToString()));
    }
}