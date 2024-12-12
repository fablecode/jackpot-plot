namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class LotteriesCountry
{
    public int LotteryId { get; set; }

    public int CountryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual Lottery Lottery { get; set; } = null!;
}
