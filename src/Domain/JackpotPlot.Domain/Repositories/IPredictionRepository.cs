using JackpotPlot.Domain.Domain;
using JackpotPlot.Domain.ValueObjects;

namespace JackpotPlot.Domain.Repositories;

public interface IPredictionRepository
{
    Task<PredictionDomain> Add(PredictionResult predictionResult);
}