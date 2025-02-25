namespace JackpotPlot.Prediction.API.Application.Models.Output;

public record HotColdNumbersOutput(Dictionary<int, int> HotNumbers, Dictionary<int, int> ColdNumbers);