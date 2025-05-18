using JackpotPlot.Domain.Models;
using MediatR;

namespace JackpotPlot.Lottery.API.Application.Features.GetTickets;

public record GetTicketsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? UserId = null,
    string? SearchTerm = null,
    string SortColumn = "ticket_id",
    string SortDirection = "asc"
) : IRequest<Result<PagedTickets>>;