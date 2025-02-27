using JackpotPlot.Prediction.API.Application.Features.GetHotAndColdNumbers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Prediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotteriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LotteriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get All Hot & Cold Numbers
    /// </summary>
    /// <returns></returns>
    [HttpGet("hot-cold-numbers")]
    public async Task<IActionResult> GetHotAndColdNumbers()
    {
        var result = await _mediator.Send(new GetHotAndColdNumbersQuery());

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }
}