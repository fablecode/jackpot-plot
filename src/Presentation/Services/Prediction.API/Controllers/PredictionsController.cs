using JackpotPlot.Domain.Constants;
using JackpotPlot.Prediction.API.Application.Features.PredictNext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prediction.API.Models;

namespace Prediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PredictionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get Prediction by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}", Name = nameof(GetResourceById))]
    public IActionResult GetResourceById(int id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Predict numbers for the next draw.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PredictNextRequest request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
        {
            return CreatedAtRoute(nameof(GetResourceById), new { id = result.Value.Id }, new { id = result.Value.Id });
        }

        return BadRequest(result.Errors);
    }

    [HttpGet("strategies")]
    public IActionResult GetStrategies()
    {
        var strategies = new List<PredictionStrategy>
        {
            new(PredictionStrategyType.Random, "Generate numbers randomly."),
            new(PredictionStrategyType.FrequencyBased, "Predict numbers based on historical frequency." ),
            new(PredictionStrategyType.AiBased, "Use advanced AI algorithms to predict numbers.")
        };

        return Ok(strategies);
    }
}