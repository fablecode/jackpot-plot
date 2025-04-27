using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using JackpotPlot.Lottery.API.Application.Extensions;
using JackpotPlot.Lottery.API.Application.Models.Output;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetTicketById;

public sealed class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, Result<TicketOutput>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }
    public async Task<Result<TicketOutput>> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _ticketRepository.GetTicketById(request.Id);

        return result == null 
            ? Result<TicketOutput>.Failure($"Ticket with id {request.Id} not found.") 
            : Result<TicketOutput>.Success(result.ToOutput());
    }
}