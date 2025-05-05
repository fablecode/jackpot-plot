using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;

namespace JackpotPlot.Domain.Repositories;

public interface ITicketPlayRepository
{
    Task<ImmutableArray<Guid>> Add(Guid ticketId, ImmutableArray<(int LineIndex, List<int> Numbers)> ticketPlays);
    Task<bool> Delete(params Guid[] ids);
}