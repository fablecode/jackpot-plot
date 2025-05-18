namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class TicketOverview
{
    public Guid? UserId { get; set; }

    public Guid? TicketId { get; set; }

    public string? TicketName { get; set; }

    public string? LotteryName { get; set; }

    public long? Entries { get; set; }
}
