namespace JackpotPlot.Prediction.API.Infrastructure.Models;

public partial class Prediction
{
    public int Id { get; set; }

    public int? Userid { get; set; }

    public int Lotteryid { get; set; }

    public List<int> Predictionnumbers { get; set; } = null!;

    public decimal? Confidencescore { get; set; }

    public DateTime? Generatedat { get; set; }
}
