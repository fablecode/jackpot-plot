namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class Country
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public int ContinentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Continent Continent { get; set; } = null!;

    public virtual ICollection<LotteriesCountry> LotteriesCountries { get; set; } = new List<LotteriesCountry>();
}
