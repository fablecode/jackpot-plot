using System.Collections.Immutable;
using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Models.Input;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.AddTicketPlays;

public record AddTicketPlaysRequest(Guid TicketId, CreateTicketPlaysInput[] Plays) : IRequest<Result<ImmutableArray<Guid>>>;