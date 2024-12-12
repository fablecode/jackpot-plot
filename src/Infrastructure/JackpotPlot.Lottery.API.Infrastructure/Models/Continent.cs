namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class Continent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Country> Countries { get; set; } = new List<Country>();
}
