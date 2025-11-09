using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Predictions.Helpers;

public sealed class MixedAlgorithmHelpers
{
    public static IEnumerable<int> CombineMainNumbers(List<(PredictionResult Result, double Weight)> results, int take, Random rng)
    {
        if (take <= 0) yield break;

        var weights = new Dictionary<int, double>();

        foreach (var (res, w) in results)
        {
            var weight = w > 0 ? w : 0;
            foreach (var n in res.PredictedNumbers)
            {
                if (!weights.TryAdd(n, weight))
                    weights[n] += weight;
            }
        }

        // order by weight desc; break ties with randomness for variety
        foreach (var n in weights
                     .OrderByDescending(kv => kv.Value)
                     .ThenBy(_ => rng.Next())
                     .Select(kv => kv.Key)
                     .Take(take))
        {
            yield return n;
        }
    }

    public static IEnumerable<int> CombineBonusNumbers(List<(PredictionResult Result, double Weight)> results, int take)
    {
        if (take <= 0) yield break;

        var counts = results
            .SelectMany(t => t.Result.BonusNumbers)
            .GroupBy(n => n)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(take);

        foreach (var n in counts) yield return n;
    }

    public static double WeightedAverageConfidence(List<(PredictionResult Result, double Weight)> results)
    {
        double total = 0, wsum = 0;
        foreach (var (r, w) in results)
        {
            total += r.ConfidenceScore * w;
            wsum += w;
        }
        return wsum == 0 ? 0 : total / wsum;
    }
}