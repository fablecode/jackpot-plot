using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;

namespace JackpotPlot.Domain.Repositories;

public interface ITicketRepository
{
    Task<ImmutableArray<TicketDomain>> GetAllUserTickets(Guid userId);
    Task<TicketDomain?> GetTicketById(Guid id);
    Task<Guid> Add(TicketDomain ticket);
}