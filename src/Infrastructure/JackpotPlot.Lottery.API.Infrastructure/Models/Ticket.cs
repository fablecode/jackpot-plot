namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class Ticket
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TicketPlay> TicketPlays { get; set; } = new List<TicketPlay>();
}
