namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class Draw
{
    public int Id { get; set; }

    public int? LotteryId { get; set; }

    public DateOnly DrawDate { get; set; }

    public TimeOnly? DrawTime { get; set; }

    public decimal? JackpotAmount { get; set; }

    public int? RolloverCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DrawResult> DrawResults { get; set; } = new List<DrawResult>();

    public virtual Lottery? Lottery { get; set; }
}
