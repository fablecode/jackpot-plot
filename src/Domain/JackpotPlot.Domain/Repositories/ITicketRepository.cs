using System.Collections.Immutable;
using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.Models;

namespace JackpotPlot.Domain.Repositories;

public interface ITicketRepository
{
    Task<ImmutableArray<TicketDomain>> GetAllUserTickets(Guid userId);
    Task<TicketDomain?> GetTicketById(Guid id);
    Task<Guid> Add(TicketDomain ticket);

    Task<PagedTickets> SearchTickets(
        int pageNumber,
        int pageSize,
        Guid? userId,
        string? searchTerm = null,
        string sortColumn = "ticket_id",
        string sortDirection = "asc");
}