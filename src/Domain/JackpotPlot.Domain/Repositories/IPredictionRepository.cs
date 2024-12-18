using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Repositories;

public interface IPredictionRepository
{
    Task<int> Add(PredictionResult predictionResult);
}