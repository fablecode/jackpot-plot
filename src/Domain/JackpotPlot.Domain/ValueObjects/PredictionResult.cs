using System.Collections.Immutable;

namespace JackpotPlot.Domain.ValueObjects;

public record PredictionResult(int LotteryId, ImmutableArray<int> PredictedNumbers, ImmutableArray<int> BonusNumbers, double ConfidenceScore, string Strategy);