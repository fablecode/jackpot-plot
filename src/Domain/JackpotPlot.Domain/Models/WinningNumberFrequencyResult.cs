namespace JackpotPlot.Domain.Models;

public class WinningNumberFrequencyResult
{
    public int Number { get; set; }
    public Dictionary<string, int> FrequencyOverTime { get; set; }
}