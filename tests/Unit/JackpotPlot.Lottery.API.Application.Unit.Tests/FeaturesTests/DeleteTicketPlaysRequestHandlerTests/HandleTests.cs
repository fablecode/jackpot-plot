using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Lottery.API.Application.Features.DeleteTicketPlays;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.DeleteTicketPlaysRequestHandlerTests;

[TestFixture]
public class HandleTests
{
    private ITicketPlayRepository _ticketPlayRepository = null!;
    private DeleteTicketPlaysRequestHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _ticketPlayRepository = Substitute.For<ITicketPlayRepository>();
        _sut = new DeleteTicketPlaysRequestHandler(_ticketPlayRepository);
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Call_Repository_Delete_With_PlayIds()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _ticketPlayRepository.Delete(Arg.Any<Guid[]>())
                             .Returns(Task.FromResult(true));

        var request = new DeleteTicketPlaysRequest(ticketId, playIds);

        // Act
        _ = await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _ticketPlayRepository.Received(1).Delete(
            Arg.Is<Guid[]>(ids => ids.SequenceEqual(playIds)));
    }

    [Test]
    public async Task Given_Repository_Returns_True_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid() };

        _ticketPlayRepository.Delete(playIds)
                             .Returns(Task.FromResult(true));

        var request = new DeleteTicketPlaysRequest(ticketId, playIds);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Returns_True_When_Handle_Is_Invoked_Should_Return_True_Value()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _ticketPlayRepository.Delete(playIds)
                             .Returns(Task.FromResult(true));

        var request = new DeleteTicketPlaysRequest(ticketId, playIds);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Value.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Returns_False_When_Handle_Is_Invoked_Should_Return_Failure_Result()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid() };

        _ticketPlayRepository.Delete(playIds)
                             .Returns(Task.FromResult(false));

        var request = new DeleteTicketPlaysRequest(ticketId, playIds);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task Given_Repository_Returns_False_When_Handle_Is_Invoked_Should_Contain_TicketId_In_Error_Message()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var playIds = new[] { Guid.NewGuid() };

        _ticketPlayRepository.Delete(playIds)
                             .Returns(Task.FromResult(false));

        var request = new DeleteTicketPlaysRequest(ticketId, playIds);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(err => err.Contains(ticketId.ToString()));
    }
}