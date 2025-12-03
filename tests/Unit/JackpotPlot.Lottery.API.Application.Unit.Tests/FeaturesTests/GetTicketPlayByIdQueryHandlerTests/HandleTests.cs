using FluentAssertions;
using JackpotPlot.Lottery.API.Application.Features.GetTicketPlayById;
using NUnit.Framework;

namespace JackpotPlot.Lottery.API.Application.Unit.Tests.FeaturesTests.GetTicketPlayByIdQueryHandlerTests;

[TestFixture]
public class HandleTests
{
    private GetTicketPlayByIdQueryHandler _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new GetTicketPlayByIdQueryHandler();
    }

    [Test]
    public async Task Given_Valid_Request_When_Handle_Is_Invoked_Should_Throw_NotImplementedException()
    {
        // Arrange
        var request = new GetTicketPlayByIdQuery(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotImplementedException>();
    }
}