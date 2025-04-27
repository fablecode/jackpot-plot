namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class TicketPlay
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public List<int> Numbers { get; set; } = null!;

    public int LineIndex { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
