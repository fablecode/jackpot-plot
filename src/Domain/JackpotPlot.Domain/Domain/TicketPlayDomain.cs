namespace JackpotPlot.Domain.Domain;

public class TicketPlayDomain
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public List<int> Numbers { get; set; } = null!;

    public int LineIndex { get; set; }

    public DateTime CreatedAt { get; set; }

    public TicketDomain Ticket { get; set; } = null!;
}