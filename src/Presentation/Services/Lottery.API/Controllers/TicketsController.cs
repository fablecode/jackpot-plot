using JackpotPlot.Lottery.API.Application.Features.CreateUserTicket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JackpotPlot.Lottery.API.Application.Features.GetAllUserTickets;
using JackpotPlot.Lottery.API.Application.Features.GetTicketById;
using JackpotPlot.Lottery.API.Application.Features.GetTickets;
using JackpotPlot.Lottery.API.Application.Models.Input;
using Lottery.API.Models.Input;

namespace Lottery.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve all user tickets
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();

        var result = await _mediator.Send(new GetAllUserTicketsQuery(userId));

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
        var result = await _mediator.Send(new GetTicketByIdQuery(id));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    /// <summary>
    /// Search for tickets
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> Get([FromQuery] TicketOverviewInput input)
    {
        // Optionally extract user ID from JWT or claims
        Guid? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var parsed))
            {
                userId = parsed;
            }
        }

        var result = await _mediator.Send(new GetTicketsQuery(input.PageNumber, input.PageSize, userId, input.SearchTerm, input.SortColumn, input.SortDirection));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NoContent();
    }

    /// <summary>
    /// Create new ticket
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateTicketInput input)
    {
        var userId = GetUserId();

        var result = await _mediator.Send(new CreateUserTicketRequest(userId, input));

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, null);
        }

        return BadRequest(new { errors = result.Errors });
    }

    #region Helpers

    private Guid GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier or "sub");
        return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : throw new UnauthorizedAccessException();
    }

    #endregion
}