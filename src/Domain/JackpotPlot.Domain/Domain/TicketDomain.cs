namespace JackpotPlot.Domain.Domain;

public class TicketDomain
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<TicketPlayDomain> UserTicketPlays { get; set; } = new List<TicketPlayDomain>();
}