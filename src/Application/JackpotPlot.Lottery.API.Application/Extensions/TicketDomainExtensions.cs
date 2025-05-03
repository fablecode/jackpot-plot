using JackpotPlot.Domain.Domain;
using JackpotPlot.Lottery.API.Application.Models.Output;

namespace JackpotPlot.Lottery.API.Application.Extensions;

public static class TicketDomainExtensions
{
    public static TicketOutput ToOutput(this TicketDomain ticket)
    {
        return new TicketOutput(ticket.Id, ticket.Name, ticket.IsPublic, ticket.UserTicketPlays.Count);
    }
}