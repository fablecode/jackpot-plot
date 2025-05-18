namespace Lottery.API.Models.Input;

public class TicketOverviewInput
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string SortColumn { get; set; } = "ticket_id";
    public string SortDirection { get; set; } = "asc";
}