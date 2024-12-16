using Microsoft.AspNetCore.Mvc;

namespace Prediction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionsController : ControllerBase
{
    /// <summary>
    /// Predict numbers for the next draw.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public IActionResult Post([FromBody] PredictNextInput input)
    {
        return Ok(new { Message = "PredictNext endpoint is working!" });
    }

    [HttpGet("strategies")]
    public IActionResult GetStrategies()
    {
        var strategies = new List<PredictionStrategy>
        {
            new("random", "Generate numbers randomly."),
            new("frequency-based", "Predict numbers based on historical frequency." ),
            new("ai-based", "Use advanced AI algorithms to predict numbers.")
        };

        return Ok(strategies);
    }
}

public record PredictionStrategy(string Name, string Description);
public record PredictNextInput(int LotteryId, int? UserId = null, string Strategy = "random");