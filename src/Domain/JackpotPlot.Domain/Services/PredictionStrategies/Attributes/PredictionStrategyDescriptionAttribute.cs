namespace JackpotPlot.Domain.Services.PredictionStrategies.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PredictionStrategyDescriptionAttribute(string id, string description) : Attribute
{
    public string Id { get; init; } = id;
    public string Description { get; init; } = description;
}
