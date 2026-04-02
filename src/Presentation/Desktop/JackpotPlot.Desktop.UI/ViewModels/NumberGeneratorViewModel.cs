using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JackpotPlot.Desktop.UI.Services.Api;
using JackpotPlot.Desktop.UI.Services.Api.Models;
using JackpotPlot.Desktop.UI.Services.Navigation;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using System.Linq;

namespace JackpotPlot.Desktop.UI.ViewModels;

public sealed partial class NumberGeneratorViewModel : ViewModelBase, INavigationAware<NumberGeneratorNavigationRequest>, IHasNavigationKey
{
    private readonly IPredictionsApiClient _predictionsApiClient;
    private readonly ILotteryApiClient _lotteryApiClient;

    public NumberGeneratorViewModel(
        IPredictionsApiClient predictionsApiClient,
        ILotteryApiClient lotteryApiClient)
    {
        _predictionsApiClient = predictionsApiClient;
        _lotteryApiClient = lotteryApiClient;

        // Initialize number of plays options (1-10, 15, 20, 25)
        NumberOfPlaysOptions = new ObservableCollection<int>(
            Enumerable.Range(1, 10).Concat([15, 20, 25])
        );

        // Set defaults
        SelectedNumberOfPlays = 5;
    }

    #region Navigation

    public string NavigationKey => NavigationKeys.NumberGenerator;

    public string Title => "Number Generator";

    public string Summary => "Generate lottery number predictions using various strategies.";

    public async Task OnNavigatedToAsync(NumberGeneratorNavigationRequest request, CancellationToken cancellationToken = default)
    {
        // Load initial data
        await LoadLotteriesAsync(cancellationToken);
        await LoadStrategiesAsync(cancellationToken);

        // Apply pre-selected values if provided
        if (request.PreSelectedLotteryId.HasValue)
        {
            SelectedLottery = Lotteries.FirstOrDefault(l => l.Id == request.PreSelectedLotteryId.Value);
        }

        if (!string.IsNullOrEmpty(request.PreSelectedStrategy))
        {
            SelectedStrategy = Strategies.FirstOrDefault(s => s.Id == request.PreSelectedStrategy);
        }
    }

    #endregion

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<LotteryDto> _lotteries = [];

    [ObservableProperty]
    private ObservableCollection<StrategyDto> _strategies = [];

    [ObservableProperty]
    private ObservableCollection<int> _numberOfPlaysOptions = [];

    [ObservableProperty]
    private LotteryDto? _selectedLottery;

    [ObservableProperty]
    private int _selectedNumberOfPlays;

    [ObservableProperty]
    private StrategyDto? _selectedStrategy;

    [ObservableProperty]
    private PredictNextResponse? _searchResults;

    [ObservableProperty]
    private bool _isLoadingPredictions;

    [ObservableProperty]
    private bool _isLoadingLotteries;

    [ObservableProperty]
    private bool _isLoadingStrategies;

    [ObservableProperty]
    private bool _showCharts;

    [ObservableProperty]
    private string? _errorMessage;

    // Chart Data Properties
    [ObservableProperty]
    private HotColdNumbersOutput? _hotColdNumbers;

    [ObservableProperty]
    private Dictionary<int, int>? _trendingNumbers;

    [ObservableProperty]
    private NumberSpreadResult? _numberSpread;

    [ObservableProperty]
    private List<LuckyPairResult>? _luckyPairFrequency;

    // LiveCharts2 Series
    [ObservableProperty]
    private ISeries[] _trendingNumbersSeries = [];

    [ObservableProperty]
    private Axis[] _trendingNumbersXAxes = 
    [
        new Axis
        {
            Name = "Numbers",
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 15)
        }
    ];

    [ObservableProperty]
    private Axis[] _trendingNumbersYAxes = 
    [
        new Axis
        {
            Name = "Frequency",
            NamePadding = new LiveChartsCore.Drawing.Padding(15, 0)
        }
    ];

    [ObservableProperty]
    private ISeries[] _hotColdNumbersSeries = [];

    [ObservableProperty]
    private Axis[] _hotColdNumbersXAxes = 
    [
        new Axis
        {
            Name = "Number",
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 15)
        }
    ];

    [ObservableProperty]
    private Axis[] _hotColdNumbersYAxes = 
    [
        new Axis
        {
            Name = "Frequency",
            NamePadding = new LiveChartsCore.Drawing.Padding(15, 0)
        }
    ];

    #endregion

    #region Computed Properties

    public bool CanGenerate => SelectedLottery != null && SelectedStrategy != null && !IsLoadingPredictions;

    partial void OnSelectedLotteryChanged(LotteryDto? value)
    {
        GenerateNumbersCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedStrategyChanged(StrategyDto? value)
    {
        GenerateNumbersCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsLoadingPredictionsChanged(bool value)
    {
        GenerateNumbersCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Commands

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateNumbersAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedLottery == null || SelectedStrategy == null)
            return;

        try
        {
            IsLoadingPredictions = true;
            ErrorMessage = null;
            SearchResults = null;
            ShowCharts = false;

            // Generate predictions
            var request = new PredictNextRequest(
                LotteryId: SelectedLottery.Id,
                NumberOfPlays: SelectedNumberOfPlays,
                Strategy: SelectedStrategy.Id ?? "random",
                UserId: null // TODO: Get from authentication service
            );

            var response = await _predictionsApiClient.GeneratePredictionsAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                SearchResults = response.Content;
                ShowCharts = true;

                // Load chart data in parallel
                await LoadChartsAsync(cancellationToken);
            }
            else
            {
                ErrorMessage = $"Failed to generate predictions: {response.Error?.Content}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsLoadingPredictions = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedLottery = null;
        SelectedNumberOfPlays = 5;
        SelectedStrategy = Strategies.FirstOrDefault(s => s.Id == "random");
        SearchResults = null;
        ShowCharts = false;
        ErrorMessage = null;

        // Clear chart data
        HotColdNumbers = null;
        TrendingNumbers = null;
        NumberSpread = null;
        LuckyPairFrequency = null;

        // Clear chart series
        TrendingNumbersSeries = [];
        HotColdNumbersSeries = [];
    }

    #endregion

    #region Private Methods

    private async Task LoadLotteriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoadingLotteries = true;

            var response = await _lotteryApiClient.GetAllLotteriesAsync(cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                Lotteries = new ObservableCollection<LotteryDto>(response.Content);
            }
            else
            {
                ErrorMessage = $"Failed to load lotteries: {response.Error?.Content}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading lotteries: {ex.Message}";
        }
        finally
        {
            IsLoadingLotteries = false;
        }
    }

    private async Task LoadStrategiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoadingStrategies = true;

            var response = await _predictionsApiClient.GetStrategiesAsync(cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                Strategies = new ObservableCollection<StrategyDto>(response.Content);

                // Set default strategy to "random" if available
                SelectedStrategy = Strategies.FirstOrDefault(s => s.Id == "random") ?? Strategies.FirstOrDefault();
            }
            else
            {
                ErrorMessage = $"Failed to load strategies: {response.Error?.Content}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading strategies: {ex.Message}";
        }
        finally
        {
            IsLoadingStrategies = false;
        }
    }

    private async Task LoadChartsAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedLottery == null)
            return;

        try
        {
            // Load all chart data in parallel
            var hotColdTask = LoadHotColdNumbersAsync(SelectedLottery.Id, cancellationToken);
            var trendingTask = LoadTrendingNumbersAsync(cancellationToken);
            var spreadTask = LoadNumberSpreadAsync(cancellationToken);
            var luckyPairTask = LoadLuckyPairFrequencyAsync(cancellationToken);

            await Task.WhenAll(hotColdTask, trendingTask, spreadTask, luckyPairTask);
        }
        catch (Exception ex)
        {
            // Log error but don't block the main flow
            ErrorMessage = $"Error loading chart data: {ex.Message}";
        }
    }

    private async Task LoadHotColdNumbersAsync(int lotteryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _predictionsApiClient.GetHotColdNumbersAsync(lotteryId, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                HotColdNumbers = response.Content;
                UpdateHotColdNumbersChart();
            }
        }
        catch (Exception ex)
        {
            // Silently fail for chart data
            System.Diagnostics.Debug.WriteLine($"Error loading hot/cold numbers: {ex.Message}");
        }
    }

    private async Task LoadTrendingNumbersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _predictionsApiClient.GetTrendingNumbersAsync(cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                TrendingNumbers = response.Content;
                UpdateTrendingNumbersChart();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading trending numbers: {ex.Message}");
        }
    }

    private void UpdateTrendingNumbersChart()
    {
        if (TrendingNumbers == null || TrendingNumbers.Count == 0)
        {
            TrendingNumbersSeries = [];
            return;
        }

        // Convert dictionary to sorted array of values for the bar chart
        var sortedData = TrendingNumbers
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => new { Number = kvp.Key, Frequency = kvp.Value })
            .ToArray();

        TrendingNumbersSeries = 
        [
            new ColumnSeries<int>
            {
                Name = "Trending Numbers",
                Values = sortedData.Select(d => d.Frequency).ToArray(),
                Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(59, 130, 246)), // Blue color #3B82F6
                DataLabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(75, 85, 99)),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End
            }
        ];

        // Update X-axis with number labels
        TrendingNumbersXAxes = 
        [
            new Axis
            {
                Name = "Numbers",
                Labels = sortedData.Select(d => d.Number.ToString()).ToArray(),
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 15)
            }
        ];
    }

    private void UpdateHotColdNumbersChart()
    {
        if (HotColdNumbers == null)
        {
            HotColdNumbersSeries = [];
            return;
        }

        // Get all unique numbers from both hot and cold
        var allNumbers = HotColdNumbers.HotNumbers.Keys
            .Concat(HotColdNumbers.ColdNumbers.Keys)
            .Distinct()
            .OrderBy(n => n)
            .ToArray();

        // Create data arrays, using 0 for numbers not in the respective dictionary
        var hotData = allNumbers
            .Select(num => HotColdNumbers.HotNumbers.GetValueOrDefault(num, 0))
            .ToArray();

        var coldData = allNumbers
            .Select(num => HotColdNumbers.ColdNumbers.GetValueOrDefault(num, 0))
            .ToArray();

        HotColdNumbersSeries = 
        [
            new ColumnSeries<int>
            {
                Name = "Hot Numbers 🔥",
                Values = hotData,
                Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(239, 68, 68)), // Red color #EF4444
                Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(220, 38, 38)) { StrokeThickness = 2 },
                DataLabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(75, 85, 99)),
                DataLabelsSize = 11,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End
            },
            new ColumnSeries<int>
            {
                Name = "Cold Numbers ❄️",
                Values = coldData,
                Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(59, 130, 246)), // Blue color #3B82F6
                Stroke = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(29, 78, 216)) { StrokeThickness = 2 },
                DataLabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(75, 85, 99)),
                DataLabelsSize = 11,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End
            }
        ];

        // Update X-axis with number labels
        HotColdNumbersXAxes = 
        [
            new Axis
            {
                Name = "Number",
                Labels = allNumbers.Select(n => n.ToString()).ToArray(),
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 15)
            }
        ];
    }

    private async Task LoadNumberSpreadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _predictionsApiClient.GetNumberSpreadAsync(cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                NumberSpread = response.Content;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading number spread: {ex.Message}");
        }
    }

    private async Task LoadLuckyPairFrequencyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _predictionsApiClient.GetLuckyPairFrequencyAsync(cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                LuckyPairFrequency = response.Content;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading lucky pair frequency: {ex.Message}");
        }
    }

    #endregion
}
