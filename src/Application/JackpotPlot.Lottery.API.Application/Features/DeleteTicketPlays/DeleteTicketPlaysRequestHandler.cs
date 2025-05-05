using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Repositories;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.DeleteTicketPlays;

public sealed class DeleteTicketPlaysRequestHandler : IRequestHandler<DeleteTicketPlaysRequest, Result<bool>>
{
    private readonly ITicketPlayRepository _ticketPlayRepository;

    public DeleteTicketPlaysRequestHandler(ITicketPlayRepository ticketPlayRepository)
    {
        _ticketPlayRepository = ticketPlayRepository;
    }

    public async Task<Result<bool>> Handle(DeleteTicketPlaysRequest request, CancellationToken cancellationToken)
    {
        var result = await _ticketPlayRepository.Delete(request.PlayIds);

        return result
            ? Result<bool>.Success(result)
            : Result<bool>.Failure($"Not able to delete plays from ticket id '{request.TicketId}'.");
    }
}