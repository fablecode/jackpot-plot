using JackpotPlot.Lottery.API.Application.Features.AddPlay;
using JackpotPlot.Lottery.API.Application.Features.GetTicketPlayById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lottery.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/{ticketId:guid}/plays")]
public class UserTicketPlaysController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserTicketPlaysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve plays for ticket by id
    /// </summary>
    /// <param name="ticketId"></param>
    /// <returns></returns>
    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> Get(Guid ticketId)
    {
        var result = await _mediator.Send(new GetTicketPlayByIdQuery(ticketId));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    /// <summary>
    /// Add plays for ticket
    /// </summary>
    /// <param name="ticketId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post(Guid ticketId, [FromBody] AddPlayRequest request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, null);
    }
}