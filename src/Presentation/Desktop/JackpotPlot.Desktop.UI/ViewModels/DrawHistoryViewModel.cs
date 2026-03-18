using CommunityToolkit.Mvvm.ComponentModel;
using JackpotPlot.Desktop.UI.Services.Navigation;
using System.Collections.ObjectModel;

namespace JackpotPlot.Desktop.UI.ViewModels;

public sealed partial class DrawHistoryViewModel : ViewModelBase, INavigationAware<DrawHistoryNavigationRequest>, IHasNavigationKey
{
    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private DateOnly? _fromDate;

    [ObservableProperty]
    private DateOnly? _toDate;

    public DrawHistoryViewModel()
    {
        LoadFakeData();
    }

    public string NavigationKey => NavigationKeys.DrawHistory;

    public string Title => "Draw History";

    public string Summary => "Historical lottery draw results and winning numbers.";

    public ObservableCollection<DrawHistoryItem> DrawHistory { get; } = new();

    public Task OnNavigatedToAsync(DrawHistoryNavigationRequest request, CancellationToken cancellationToken = default)
    {
        SearchText = request.SearchText;
        FromDate = request.From;
        ToDate = request.To;

        return Task.CompletedTask;
    }

    private void LoadFakeData()
    {
        var fakeDraws = new[]
        {
            new DrawHistoryItem(
                DrawDate: new DateOnly(2024, 1, 15),
                WinningNumbers: [4, 11, 19, 26, 37, 44],
                JackpotAmount: 125_000_000m,
                WinnersCount: 2,
                LotteryName: "Powerball"),

            new DrawHistoryItem(
                DrawDate: new DateOnly(2024, 1, 12),
                WinningNumbers: [7, 14, 21, 28, 35, 42],
                JackpotAmount: 95_000_000m,
                WinnersCount: 0,
                LotteryName: "Powerball"),

            new DrawHistoryItem(
                DrawDate: new DateOnly(2024, 1, 10),
                WinningNumbers: [3, 16, 23, 29, 31, 45],
                JackpotAmount: 87_500_000m,
                WinnersCount: 1,
                LotteryName: "Powerball"),

            new DrawHistoryItem(
                DrawDate: new DateOnly(2024, 1, 8),
                WinningNumbers: [2, 9, 18, 27, 33, 41],
                JackpotAmount: 78_000_000m,
                WinnersCount: 0,
                LotteryName: "Powerball"),

            new DrawHistoryItem(
                DrawDate: new DateOnly(2024, 1, 5),
                WinningNumbers: [5, 12, 20, 25, 38, 43],
                JackpotAmount: 65_000_000m,
                WinnersCount: 3,
                LotteryName: "Powerball"),

            new DrawHistoryItem(
                DrawDate: new DateOnly(2024, 1, 3),
                WinningNumbers: [1, 8, 15, 22, 34, 40],
                JackpotAmount: 52_000_000m,
                WinnersCount: 1,
                LotteryName: "Powerball"),

            new DrawHistoryItem(
                DrawDate: new DateOnly(2023, 12, 30),
                WinningNumbers: [6, 13, 17, 24, 36, 46],
                JackpotAmount: 145_000_000m,
                WinnersCount: 0,
                LotteryName: "Powerball"),

            new DrawHistoryItem(
                DrawDate: new DateOnly(2023, 12, 27),
                WinningNumbers: [10, 19, 21, 30, 39, 47],
                JackpotAmount: 132_000_000m,
                WinnersCount: 2,
                LotteryName: "Powerball")
        };

        foreach (var draw in fakeDraws)
        {
            DrawHistory.Add(draw);
        }
    }
}
