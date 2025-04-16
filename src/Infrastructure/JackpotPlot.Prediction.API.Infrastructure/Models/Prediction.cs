namespace JackpotPlot.Prediction.API.Infrastructure.Models;

public partial class Prediction
{
    public int Id { get; set; }

    public int LotteryId { get; set; }

    public Guid? UserId { get; set; }

    public string Strategy { get; set; } = null!;

    public List<int> PredictedNumbers { get; set; } = null!;

    public List<int>? BonusNumbers { get; set; }

    public decimal? ConfidenceScore { get; set; }

    public DateTime? CreatedAt { get; set; }
}
