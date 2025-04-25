using JackpotPlot.Lottery.API.Application.Features.CreateUserTicket;
using JackpotPlot.Lottery.API.Application.Features.GetAllUserTickets;
using JackpotPlot.Lottery.API.Application.Features.GetUserTicketById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lottery.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserTicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserTicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve all tickets
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetAllUserTicketsQuery());

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    /// <summary>
    /// Retrieve ticket by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetUserTicketByIdQuery(id));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    /// <summary>
    /// Create new ticket
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserTicketRequest request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, null);
    }
}