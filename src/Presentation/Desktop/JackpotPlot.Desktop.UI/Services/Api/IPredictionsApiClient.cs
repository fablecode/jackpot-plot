using JackpotPlot.Desktop.UI.Services.Api.Models;
using Refit;

namespace JackpotPlot.Desktop.UI.Services.Api;

/// <summary>
/// Refit API client for Predictions service endpoints
/// </summary>
public interface IPredictionsApiClient
{
    /// <summary>
    /// Generate lottery number predictions
    /// </summary>
    [Post("/api/predictions")]
    Task<ApiResponse<PredictNextResponse>> GeneratePredictionsAsync(
        [Body] PredictNextRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all available prediction strategies
    /// </summary>
    [Get("/api/predictions/strategies")]
    Task<ApiResponse<List<StrategyDto>>> GetStrategiesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get hot and cold numbers for a specific lottery
    /// </summary>
    [Get("/api/predictions/hot-cold-numbers")]
    Task<ApiResponse<HotColdNumbersOutput>> GetHotColdNumbersAsync(
        [Query] int lotteryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trending numbers across all lotteries
    /// </summary>
    [Get("/api/predictions/trending-numbers")]
    Task<ApiResponse<Dictionary<int, int>>> GetTrendingNumbersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get number spread analysis (Low, Mid, High distribution)
    /// </summary>
    [Get("/api/predictions/number-spread")]
    Task<ApiResponse<NumberSpreadResult>> GetNumberSpreadAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get lucky number pair frequencies
    /// </summary>
    [Get("/api/predictions/lucky-pair-frequency")]
    Task<ApiResponse<List<LuckyPairResult>>> GetLuckyPairFrequencyAsync(
        CancellationToken cancellationToken = default);
}
