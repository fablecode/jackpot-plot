using JackpotPlot.Lottery.API.Application.Features.AddTicketPlays;
using JackpotPlot.Lottery.API.Application.Features.DeleteTicketPlays;
using JackpotPlot.Lottery.API.Application.Features.GetTicketPlayById;
using JackpotPlot.Lottery.API.Application.Models.Input;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lottery.API.Controllers;

[Authorize]
[ApiController]
[Route("api/tickets/{ticketId:guid}/[controller]")]
public class PlaysController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlaysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve plays for ticket by Id
    /// </summary>
    /// <param name="ticketId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] Guid ticketId)
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
    /// <param name="plays"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromRoute] Guid ticketId, [FromBody] CreateTicketPlaysInput[] plays)
    {
        var result = await _mediator.Send(new AddTicketPlaysRequest(ticketId, plays));

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Get), new { ticketId }, result.Value);
        }

        return BadRequest(new { errors = result.Errors });
    }

    /// <summary>
    /// Delete plays from ticket
    /// </summary>
    /// <param name="ticketId"></param>
    /// <param name="playIds"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] Guid ticketId, [FromBody] Guid[] playIds)
    {
        var result = await _mediator.Send(new DeleteTicketPlaysRequest(ticketId, playIds));

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(new { errors = result.Errors });
    }
}