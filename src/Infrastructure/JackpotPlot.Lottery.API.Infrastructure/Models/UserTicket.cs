namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class UserTicket
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserTicketPlay> UserTicketPlays { get; set; } = new List<UserTicketPlay>();
}
