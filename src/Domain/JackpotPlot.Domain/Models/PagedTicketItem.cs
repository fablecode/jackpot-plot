using JackpotPlot.Domain.Enums;

namespace JackpotPlot.Domain.Models;

public class PagedTicketItem
{
    public Guid? TicketId { get; set; }

    public string? TicketName { get; set; }

    public string? LotteryName { get; set; }

    public long? Entries { get; set; }

    public TicketStatus Status { get; set; }

    public ConfidenceLevel Confidence { get; set; }
}