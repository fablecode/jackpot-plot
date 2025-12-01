using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Application.Features.CreateUserTicket;
using JackpotPlot.Lottery.API.Application.Models.Input;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.CreateUserTicketRequestHandlerTests;

[TestFixture]
public class HandleTests
{
    private ITicketRepository _ticketRepository = null!;
    private CreateUserTicketRequestHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _ticketRepository = Substitute.For<ITicketRepository>();
        _sut = new CreateUserTicketRequestHandler(_ticketRepository);
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Call_Repository_Add_With_Mapped_Ticket()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plays = new[]
        {
                new CreateTicketPlaysInput(0, [1, 2, 3]),
                new CreateTicketPlaysInput(1, [4, 5])
            };

        var ticketInput = new CreateTicketInput("My ticket", plays)
        {
            LotteryId = 42
        };

        var newTicketId = Guid.NewGuid();
        _ticketRepository.Add(Arg.Any<TicketDomain>())
                         .Returns(Task.FromResult(newTicketId));

        var request = new CreateUserTicketRequest(userId, ticketInput);

        // Act
        _ = await _sut.Handle(request, CancellationToken.None);

        // Assert
        await _ticketRepository.Received(1).Add(
            Arg.Is<TicketDomain>(t =>
                t.UserId == userId &&
                t.LotteryId == 42 &&
                t.Name == "My ticket" &&
                t.UserTicketPlays.Count == plays.Length &&
                t.UserTicketPlays.OrderBy(p => p.LineIndex).ElementAt(0).LineIndex == 0 &&
                t.UserTicketPlays.OrderBy(p => p.LineIndex).ElementAt(0).Numbers.SequenceEqual(new[] { 1, 2, 3 }) &&
                t.UserTicketPlays.OrderBy(p => p.LineIndex).ElementAt(1).LineIndex == 1 &&
                t.UserTicketPlays.OrderBy(p => p.LineIndex).ElementAt(1).Numbers.SequenceEqual(new[] { 4, 5 })
            ));
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plays = new[] { new CreateTicketPlaysInput(0, [1, 2, 3]) };
        var ticketInput = new CreateTicketInput("Ticket A", plays) { LotteryId = 7 };

        var newTicketId = Guid.NewGuid();
        _ticketRepository.Add(Arg.Any<TicketDomain>())
                         .Returns(Task.FromResult(newTicketId));

        var request = new CreateUserTicketRequest(userId, ticketInput);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Return_Response_With_New_TicketId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plays = new[] { new CreateTicketPlaysInput(0, [1, 2, 3]) };
        var ticketInput = new CreateTicketInput("Ticket B", plays) { LotteryId = 9 };

        var newTicketId = Guid.NewGuid();
        _ticketRepository.Add(Arg.Any<TicketDomain>())
                         .Returns(Task.FromResult(newTicketId));

        var request = new CreateUserTicketRequest(userId, ticketInput);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Value.Id.Should().Be(newTicketId);
    }
}