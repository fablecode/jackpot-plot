using FluentAssertions;
using Lottery.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Lottery.API.Unit.Tests.ControllersTests.PinnedCombinationsControllerTests;

[TestFixture]
public class PostTests
{
    private IHttpContextAccessor _httpContextAccessor;
    private PinnedCombinationsController _sut;

    [SetUp]
    public void SetUp()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new PinnedCombinationsController(_httpContextAccessor);
    }

    [Test]
    public void Given_Valid_Request_When_Post_Is_Invoked_Should_Return_CreatedResult()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var request = new PinCombinationRequest(
            LotteryId: 1,
            PredictionId: 2,
            Numbers: [1, 2, 3, 4, 5],
            PinnedDate: DateTime.UtcNow);

        // Act
        var result = _sut.Post(request);

        // Assert
        result.Should().BeOfType<CreatedResult>();
    }

    [Test]
    public void Given_Any_Request_When_Post_Is_Invoked_Should_Return_Location_As_Won_Dot_Com()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var request = new PinCombinationRequest(
            LotteryId: 1,
            PredictionId: 2,
            Numbers: [1, 2, 3, 4, 5],
            PinnedDate: DateTime.UtcNow);

        // Act
        var result = (CreatedResult)_sut.Post(request);

        // Assert
        result.Location.Should().Be("http://won.com/");
    }

    [Test]
    public void Given_Any_Request_When_Post_Is_Invoked_Should_Return_Value_One()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var request = new PinCombinationRequest(
            LotteryId: 1,
            PredictionId: 2,
            Numbers: [1, 2, 3, 4, 5],
            PinnedDate: DateTime.UtcNow);

        // Act
        var result = (CreatedResult)_sut.Post(request);

        // Assert
        result.Value.Should().Be(1);
    }

    [Test]
    public void Given_Null_HttpContext_When_Post_Is_Invoked_Should_Still_Return_CreatedResult()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var request = new PinCombinationRequest(
            LotteryId: 1,
            PredictionId: 2,
            Numbers: [1, 2, 3, 4, 5],
            PinnedDate: DateTime.UtcNow);

        // Act
        var result = _sut.Post(request);

        // Assert
        result.Should().BeOfType<CreatedResult>();
    }
}