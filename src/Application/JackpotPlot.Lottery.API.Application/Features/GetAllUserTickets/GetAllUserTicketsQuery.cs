using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Models.Output;
using MediatR;
using System.Collections.Immutable;

namespace JackpotPlot.Lottery.API.Application.Features.GetAllUserTickets;

public record GetAllUserTicketsQuery(Guid UserId) : IRequest<Result<ImmutableArray<TicketOutput>>>;