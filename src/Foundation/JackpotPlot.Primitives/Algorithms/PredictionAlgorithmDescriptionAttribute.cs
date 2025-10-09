namespace JackpotPlot.Primitives.Algorithms;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PredictionAlgorithmDescriptionAttribute(string id, string description) : Attribute
{
    public string Id { get; init; } = id;
    public string Description { get; init; } = description;
}
