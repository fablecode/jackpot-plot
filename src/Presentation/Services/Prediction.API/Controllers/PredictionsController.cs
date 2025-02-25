using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Prediction.API.Application.Features.PredictNext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.RegularExpressions;

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
            var predictions = result.Value;

            // If multiple predictions, return 201 with all predictions
            return CreatedAtAction(nameof(Post), new { count = predictions.Predictions.Length }, predictions);
        }

        return BadRequest(result.Errors);
    }

    [HttpGet("strategies")]
    public IActionResult GetStrategies()
    {
        // Get all loaded assemblies (or limit to a specific assembly if preferred)
        var strategies = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IPredictionStrategy).IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract)
            .Select(t => new
            {
                t.GetCustomAttribute<PredictionStrategyDescriptionAttribute>()?.Id,
                Name = FormatStrategyName(t.Name),
                Description = t.GetCustomAttribute<PredictionStrategyDescriptionAttribute>()?.Description
                              ?? "No description available"
            })
            .ToList();

        return Ok(strategies);
    }

    #region Private Helpers
    private string FormatStrategyName(string name)
    {
        // Remove 'PredictionStrategy' suffix if present
        name = name.EndsWith("PredictionStrategy")
            ? name.Substring(0, name.Length - "PredictionStrategy".Length)
            : name;

        // Add spaces between words (convert PascalCase to normal text)
        return Regex.Replace(name, "(?<!^)([A-Z])", " $1");
    } 
    #endregion
}