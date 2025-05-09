﻿using JackpotPlot.Domain.Interfaces;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Services.PredictionStrategies.Attributes;
using JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbersByLotteryId;
using JackpotPlot.Prediction.API.Application.Features.GetLuckyPair;
using JackpotPlot.Prediction.API.Application.Features.GetNumberSpread;
using JackpotPlot.Prediction.API.Application.Features.GetPredictionSuccessRate;
using JackpotPlot.Prediction.API.Application.Features.GetTrendingNumbers;
using JackpotPlot.Prediction.API.Application.Features.GetWinningNumberFrequency;
using JackpotPlot.Prediction.API.Application.Features.PredictNext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Prediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PredictionsController> _logger;

    public PredictionsController(IMediator mediator, ILogger<PredictionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
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
        // If the user is authenticated, extract their ID from claims
        if (User.Identity is { IsAuthenticated: true })
        {
            var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(subClaim, out var userGuid))
            {
                // Recreate the request with UserId set
                request = request with { UserId = userGuid };
            }
        }

        var result = await _mediator.Send(request);

        if (result.IsSuccess)
        {
            var predictions = result.Value;

            // If multiple predictions, return 201 with all predictions
            return CreatedAtAction(nameof(Post), new { count = predictions.Plays.Length }, predictions);
        }

        return BadRequest(new { errors = result.Errors });
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

    /// <summary>
    /// Get Hot & Cold Number by lotteryId
    /// </summary>
    /// <param name="lotteryId"></param>
    /// <returns></returns>
    [HttpGet("hot-cold-numbers")]
    public async Task<ActionResult> GetHotColdNumbers(int lotteryId)
    {
        var result = await _mediator.Send(new GetHotAndColdNumbersByLotteryIdQuery(lotteryId));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    [HttpGet("trending-numbers")]
    public async Task<ActionResult<Dictionary<int, int>>> GetTrendingNumbers()
    {
        var result = await _mediator.Send(new GetTrendingNumbersQuery());

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    [HttpGet("success-rate")]
    public async Task<ActionResult<ImmutableDictionary<int, int>>> GetPredictionSuccessRate()
    {
        var result = await _mediator.Send(new GetPredictionSuccessRateQuery());

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    [HttpGet("number-spread")]
    public async Task<ActionResult<NumberSpreadResult>> GetNumberSpread()
    {
        var result = await _mediator.Send(new GetNumberSpreadQuery());

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    [HttpGet("lucky-pair-frequency")]
    public async Task<ActionResult<LuckyPairResult>> GetLuckPairFrequency()
    {
        var result = await _mediator.Send(new GetLuckyPairQuery());

        if (result.IsSuccess)
        {
            return Ok((result.Value));
        }

        return NoContent();
    }

    [HttpGet("winning-number-frequency")]
    public async Task<ActionResult<List<WinningNumberFrequencyResult>>> GetWinningNumberFrequency()
    {
        var result = await _mediator.Send(new GetWinningNumberFrequencyQuery());

        if (result.IsSuccess)
        {
            return Ok((result.Value));
        }

        return NoContent();
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