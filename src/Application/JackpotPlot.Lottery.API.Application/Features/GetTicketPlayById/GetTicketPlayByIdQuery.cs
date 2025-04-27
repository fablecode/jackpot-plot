using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetTicketPlayById;

public record GetTicketPlayByIdQuery(Guid Id) : IRequest<Result<ImmutableArray<PlayLine>>>;