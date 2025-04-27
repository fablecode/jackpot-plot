using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetTicketById;

public record GetTicketByIdQuery(Guid Id) : IRequest<Result<TicketOutput>>;