using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.CreateUserTicket;

public sealed class CreateUserTicketRequestHandler : IRequestHandler<CreateUserTicketRequest, Result<CreateUserTicketResponse>>
{
    private readonly ITicketRepository _ticketRepository;

    public CreateUserTicketRequestHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<CreateUserTicketResponse>> Handle(CreateUserTicketRequest request, CancellationToken cancellationToken)
    {
        var newTicketId = await _ticketRepository.Add(new TicketDomain
        {
            UserId = request.UserId,
            LotteryId = request.Ticket.LotteryId,
            Name = request.Ticket.Name,
            UserTicketPlays = request.Ticket.Plays.Select(play => new TicketPlayDomain
            {
                LineIndex = play.LineIndex,
                Numbers = play.Numbers.ToList()
            })
                .ToList()
        });

        return Result<CreateUserTicketResponse>.Success(new CreateUserTicketResponse(newTicketId));
    }
}