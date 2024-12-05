using System.Collections.Immutable;
using System.Data;

namespace JackpotPlot.Domain.Models;

public class EurojackpotResult
{
    public DateTime Date { get; set; }
    public int Rollover { get; set; }
    public ImmutableArray<int> MainNumbers { get; set; }
    public ImmutableArray<int> EuroNumbers { get; set; }
    public int TotalWinners { get; set; }
    public int JackpotWinners { get; set; }
    public string JackpotAmount { get; set; }
    public ImmutableArray<DataTable> PrizeBreakdown { get; set; }
}