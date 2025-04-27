using JackpotPlot.Domain.Models;
using JackpotPlot.Lottery.API.Application.Models.Input;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.CreateUserTicket;

public record CreateUserTicketRequest(Guid UserId, CreateTicketInput Ticket) : IRequest<Result<CreateUserTicketResponse>>;