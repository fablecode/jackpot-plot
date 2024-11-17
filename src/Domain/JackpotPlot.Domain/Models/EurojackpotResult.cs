using System.Data;

namespace JackpotPlot.Domain.Models;

public class EurojackpotResult
{
    public DateTime Date { get; set; }
    public int Rollover { get; set; }
    public List<int> MainNumbers { get; set; }
    public List<int> EuroNumbers { get; set; }
    public int TotalWinners { get; set; }
    public int JackpotWinners { get; set; }
    public string JackpotAmount { get; set; }
    public List<DataTable> PrizeBreakdown { get; set; }
}