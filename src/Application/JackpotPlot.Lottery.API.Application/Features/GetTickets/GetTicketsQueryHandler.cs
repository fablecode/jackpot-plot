using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetTickets;

public sealed class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, Result<PagedTickets>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }
    public async Task<Result<PagedTickets>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var result = await _ticketRepository.SearchTickets
        (
            request.PageNumber, 
            request.PageSize, 
            request.UserId,
            request.SearchTerm, 
            request.SortColumn, 
            request.SortDirection
        );

        return Result<PagedTickets>.Success(result);
    }
}