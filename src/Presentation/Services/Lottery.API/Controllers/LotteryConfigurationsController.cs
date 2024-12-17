using JackpotPlot.Lottery.API.Application.Features.GetLotteryConfigurationByLotteryId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lottery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotteryConfigurationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LotteryConfigurationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get lottery configuration by lottery id
    /// </summary>
    /// <param name="lotteryId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get(int lotteryId)
    {
        var result = await _mediator.Send(new GetLotteryConfigurationByLotteryIdQuery(lotteryId));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }
}