using JackpotPlot.Lottery.API.Application.Features.GetLotteries;
using JackpotPlot.Lottery.API.Application.Features.GetLotteryConfigurationByLotteryId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lottery.API.Controllers;

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
    /// Get all lotteries
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetLotteriesQuery());

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    /// <summary>
    /// Get lottery configuration by lottery id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/configuration")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _mediator.Send(new GetLotteryConfigurationByLotteryIdQuery(id));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }
}