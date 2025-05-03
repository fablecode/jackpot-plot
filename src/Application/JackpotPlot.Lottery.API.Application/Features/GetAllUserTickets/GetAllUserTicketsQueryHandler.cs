using System.Collections.Immutable;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Lottery.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetAllUserTickets;

public sealed class GetAllUserTicketsQueryHandler : IRequestHandler<GetAllUserTicketsQuery, Result<ImmutableArray<TicketOutput>>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetAllUserTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<ImmutableArray<TicketOutput>>> Handle(GetAllUserTicketsQuery request, CancellationToken cancellationToken)
    {
        var result = await _ticketRepository.GetAllUserTickets(request.UserId);

        return Result<ImmutableArray<TicketOutput>>.Success([..result.Select(t => new TicketOutput(t.Id, t.Name, t.IsPublic, t.UserTicketPlays.Count))]);
    }
}