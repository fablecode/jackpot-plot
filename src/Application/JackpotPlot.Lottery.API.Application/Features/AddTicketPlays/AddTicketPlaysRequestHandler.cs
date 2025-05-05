using System.Collections.Immutable;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.AddTicketPlays;

public sealed class AddTicketPlaysRequestHandler : IRequestHandler<AddTicketPlaysRequest, Result<ImmutableArray<Guid>>>
{
    private readonly ITicketPlayRepository _ticketPlayRepository;

    public AddTicketPlaysRequestHandler(ITicketPlayRepository ticketPlayRepository)
    {
        _ticketPlayRepository = ticketPlayRepository;
    }
    public async Task<Result<ImmutableArray<Guid>>> Handle(AddTicketPlaysRequest request, CancellationToken cancellationToken)
    {
        var result = await _ticketPlayRepository.Add(request.TicketId, [..request.Plays.Select(input => (input.LineIndex, input.Numbers.ToList()))]);

        return result.IsEmpty 
            ? Result<ImmutableArray<Guid>>.Failure($"Tickets plays were not added to ticket {request.TicketId}.") 
            : Result<ImmutableArray<Guid>>.Success(result);
    }
}