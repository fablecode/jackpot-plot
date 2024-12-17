namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class Lottery
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? DrawFrequency { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Draw> Draws { get; set; } = new List<Draw>();

    public virtual ICollection<LotteriesCountry> LotteriesCountries { get; set; } = new List<LotteriesCountry>();

    public virtual ICollection<LotteryConfiguration> LotteryConfigurations { get; set; } = new List<LotteryConfiguration>();
}
