using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetTicketPlayById;

public sealed class GetTicketPlayByIdQueryHandler : IRequestHandler<GetTicketPlayByIdQuery, Result<ImmutableArray<PlayLine>>>
{
    public Task<Result<ImmutableArray<PlayLine>>> Handle(GetTicketPlayByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}