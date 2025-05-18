namespace JackpotPlot.Domain.Models;

public class PagedTickets
{
    public int TotalItems { get; set; }
    public int TotalFilteredItems { get; set; }
    public int TotalPages { get; set; }
    public List<PagedTicketItem> Tickets { get; set; } = new();
}