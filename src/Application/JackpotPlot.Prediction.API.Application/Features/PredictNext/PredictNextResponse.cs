using JackpotPlot.Prediction.API.Application.Models.Output;
using System.Collections.Immutable;

namespace JackpotPlot.Prediction.API.Application.Features.PredictNext;

public record PredictNextResponse(int LotteryId, int NumberOfPlays, string Strategy, ImmutableArray<PlayOutput> Plays);