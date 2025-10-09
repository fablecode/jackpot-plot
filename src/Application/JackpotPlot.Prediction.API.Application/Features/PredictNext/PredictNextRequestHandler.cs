using System.Collections.ObjectModel;
using JackpotPlot.Application.Abstractions.Common;
using JackpotPlot.Application.Abstractions.Persistence.Repositories;
using JackpotPlot.Domain.Models;
using JackpotPlot.Domain.Predictions;
using JackpotPlot.Prediction.API.Application.Models.Output;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace JackpotPlot.Prediction.API.Application.Features.PredictNext;

public sealed class PredictNextRequestHandler : IRequestHandler<PredictNextRequest, Result<PredictNextResponse>>
{
    private readonly ILotteryConfigurationRepository _config;
    private readonly ILotteryHistoryRepository _history;
    private readonly IPredictionRepository _predictions;
    private readonly ILotteryStatisticsRepository _stats;
    private readonly IRandomProvider _random;
    private readonly IServiceProvider _sp; // for keyed resolution

    public PredictNextRequestHandler(ILotteryConfigurationRepository config,
        ILotteryHistoryRepository history,
        IPredictionRepository predictions,
        ILotteryStatisticsRepository stats,
        IRandomProvider random,
        IServiceProvider sp)
    {
        _config = config;
        _history = history;
        _predictions = predictions;
        _stats = stats;
        _random = random;
        _sp = sp;
    }
    public async Task<Result<PredictNextResponse>> Handle(PredictNextRequest request, CancellationToken ct)
    {
        // load inputs
        var cfg = await _config.GetActiveConfigurationAsync(request.LotteryId);
        if (cfg is null) return Result<PredictNextResponse>.Failure("Config not found.");

        var draws = await _history.GetHistoricalDraws(request.LotteryId);
        if (draws.Count == 0) return Result<PredictNextResponse>.Failure("No history found.");

        // choose algorithm by key (no Handles(string))
        var algo = _sp.GetRequiredKeyedService<IPredictionAlgorithm>(request.Strategy);

        var plays = new List<PlayOutput>(request.NumberOfPlays);
        for (var i = 0; i < request.NumberOfPlays; i++)
        {
            var result = algo.Predict(cfg, new ReadOnlyCollection<HistoricalDraw>((IList<HistoricalDraw>)draws), _random.Get());
            await _predictions.Add(request.UserId, result);

            // classify (if you keep this repo-based)
            var main = await _stats.GetHotColdNumbers(request.LotteryId, result.PredictedNumbers.ToList(), TimeSpan.Zero, "main");
            var bonus = await _stats.GetHotColdNumbers(request.LotteryId, result.BonusNumbers.ToList(), TimeSpan.Zero, "bonus");

            plays.Add(new PlayOutput(i + 1,
                main.AddRange(bonus)
                    .Select(n => new PredictionNumberOutput(n.Number, n.Frequency, n.Status, n.NumberType))
                    .ToArray()));
        }

        return Result<PredictNextResponse>.Success(
            new PredictNextResponse(request.LotteryId, request.NumberOfPlays, request.Strategy, [.. plays]));
    }
}