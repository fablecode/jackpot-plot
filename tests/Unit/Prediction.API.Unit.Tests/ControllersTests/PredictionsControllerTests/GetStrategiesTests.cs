using System.Collections;
using FluentAssertions;
using JackpotPlot.Domain.Predictions.Algorithms;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Prediction.API.Controllers;

namespace Prediction.API.Unit.Tests.ControllersTests.PredictionsControllerTests;

[TestFixture]
public class GetStrategiesTests
{
    private IMediator _mediator;
    private ILogger<PredictionsController> _logger;
    private PredictionsController _sut;

    [SetUp]
    public void SetUp()
    {
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<PredictionsController>>();
        _sut = new PredictionsController(_mediator, _logger);
    }

    [Test]
    public void Given_Request_When_GetStrategies_Is_Invoked_Should_Return_Ok()
    {
        // Arrange
        _ = typeof(ConsecutiveNumbersAlgorithm); // ensure assembly/type is loaded

        // Act
        var result = _sut.GetStrategies();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public void Given_Request_When_GetStrategies_Is_Invoked_Should_Return_NonEmpty_Strategies_List()
    {
        // Arrange
        _ = typeof(ConsecutiveNumbersAlgorithm);

        // Act
        var result = (OkObjectResult)_sut.GetStrategies();
        var list = ((IEnumerable)result.Value!).Cast<object>().ToList();

        // Assert
        list.Count.Should().BeGreaterThan(0);
    }

    [Test]
    public void Given_Known_Algorithm_When_GetStrategies_Is_Invoked_Should_Contain_Formatted_Strategy_Name()
    {
        // Arrange
        _ = typeof(ConsecutiveNumbersAlgorithm);
        const string expectedName = "Consecutive Numbers";

        // Act
        var result = (OkObjectResult)_sut.GetStrategies();
        var names = ((IEnumerable)result.Value!)
            .Cast<object>()
            .Select(x => x.GetType().GetProperty("Name")!.GetValue(x)!.ToString())
            .ToList();

        // Assert
        names.Should().Contain(expectedName);
    }

    [Test]
    public void Given_Known_Algorithm_When_GetStrategies_Is_Invoked_Should_Return_Description_From_Attribute()
    {
        // Arrange
        _ = typeof(ConsecutiveNumbersAlgorithm);
        const string expectedDescription =
            "Focuses on frequently occurring consecutive pairs or sequences, assuming that numbers appearing in a chain might appear together again.";

        // Act
        var result = (OkObjectResult)_sut.GetStrategies();
        var strategies = ((IEnumerable)result.Value!).Cast<object>().ToList();

        var target = strategies.Single(x =>
            string.Equals(
                x.GetType().GetProperty("Name")!.GetValue(x)!.ToString(),
                "Consecutive Numbers",
                StringComparison.Ordinal));

        var description = target.GetType().GetProperty("Description")!.GetValue(target)!.ToString();

        // Assert
        description.Should().Be(expectedDescription);
    }
}