namespace JackpotPlot.Desktop.UI.ViewModels;

public sealed record DrawHistoryItem(
    DateOnly DrawDate,
    int[] WinningNumbers,
    decimal JackpotAmount,
    int WinnersCount,
    string LotteryName)
{
    public string WinningNumbersDisplay => string.Join(" - ", WinningNumbers);
    public string JackpotDisplay => $"${JackpotAmount:N0}";
    public string DrawDateDisplay => DrawDate.ToString("MMM dd, yyyy");
}