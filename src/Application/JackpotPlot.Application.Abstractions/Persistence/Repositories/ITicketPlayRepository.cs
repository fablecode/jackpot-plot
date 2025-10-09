using System.Collections.Immutable;

namespace JackpotPlot.Application.Abstractions.Persistence.Repositories;

public interface ITicketPlayRepository
{
    Task<ImmutableArray<Guid>> Add(Guid ticketId, ImmutableArray<(int LineIndex, List<int> Numbers)> ticketPlays);
    Task<bool> Delete(params Guid[] ids);
}