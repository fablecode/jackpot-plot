using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.DeleteTicketPlays;

public record DeleteTicketPlaysRequest(Guid TicketId, Guid[] PlayIds) : IRequest<Result<bool>>;