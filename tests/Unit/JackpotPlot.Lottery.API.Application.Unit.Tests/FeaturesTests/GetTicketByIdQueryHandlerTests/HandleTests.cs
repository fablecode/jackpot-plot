using FluentAssertions;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Application.Features.GetTicketById;
using NSubstitute;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.GetTicketByIdQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private ITicketRepository _ticketRepository = null!;
    private GetTicketByIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _ticketRepository = Substitute.For<ITicketRepository>();
        _sut = new GetTicketByIdQueryHandler(_ticketRepository);
    }

    private static TicketDomain CreateTicket(Guid id)
        => new()
        {
            Id = id,
            UserId = Guid.NewGuid(),
            LotteryId = 1,
            Name = "My ticket",
            IsPublic = true
        };

    [Test]
    public async Task Given_Query_When_Handle_Is_Invoked_Should_Call_Repository_With_Id()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketRepository.GetTicketById(id).Returns((TicketDomain?)null);

        var query = new GetTicketByIdQuery(id);

        // Act
        _ = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _ticketRepository.Received(1).GetTicketById(id);
    }

    [Test]
    public async Task Given_Ticket_Exists_When_Handle_Is_Invoked_Should_Return_Success_Result()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ticket = CreateTicket(id);

        _ticketRepository.GetTicketById(id).Returns(ticket);

        var query = new GetTicketByIdQuery(id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Given_Ticket_Exists_When_Handle_Is_Invoked_Should_Return_Mapped_TicketOutput_With_Same_Id()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ticket = CreateTicket(id);

        _ticketRepository.GetTicketById(id).Returns(ticket);

        var query = new GetTicketByIdQuery(id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Id.Should().Be(id);
    }

    [Test]
    public async Task Given_Ticket_Not_Found_When_Handle_Is_Invoked_Should_Return_Failure_Result()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketRepository.GetTicketById(id).Returns((TicketDomain?)null);

        var query = new GetTicketByIdQuery(id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task Given_Ticket_Not_Found_When_Handle_Is_Invoked_Should_Contain_Id_In_Error_Message()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketRepository.GetTicketById(id).Returns((TicketDomain?)null);

        var query = new GetTicketByIdQuery(id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(e => e.Contains(id.ToString()));
    }
}