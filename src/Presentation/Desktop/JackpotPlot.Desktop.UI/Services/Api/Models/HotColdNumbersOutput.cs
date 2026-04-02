namespace JackpotPlot.Desktop.UI.Services.Api.Models;

/// <summary>
/// Hot and cold numbers for a lottery
/// </summary>
public record HotColdNumbersOutput(
    Dictionary<int, int> HotNumbers,
    Dictionary<int, int> ColdNumbers);
