using System.Collections.Immutable;
using System.Data;
using System.Globalization;

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

    public decimal GetJackpotAmount()
    {
        if (string.IsNullOrWhiteSpace(JackpotAmount))
        {
            return default;
        }

        // Extract the numeric part and the prefix dynamically
        var index = 0;

        // Identify where the numeric part begins
        while (index < JackpotAmount.Length && !char.IsDigit(JackpotAmount[index]))
        {
            index++;
        }

        // Extract the numeric part
        var numericPart = JackpotAmount.Substring(index);

        // Remove commas from the numeric part
        numericPart = numericPart.Replace(",", "");

        return decimal.Parse(numericPart, NumberStyles.AllowThousands);
    }
}