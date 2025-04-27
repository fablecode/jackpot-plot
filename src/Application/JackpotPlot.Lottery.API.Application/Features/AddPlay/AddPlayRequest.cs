using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.AddPlay;

public record AddPlayRequest() : IRequest<Result<TicketPlayOutput>>;