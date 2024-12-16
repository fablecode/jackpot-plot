using Microsoft.AspNetCore.Mvc;

namespace Lottery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotteriesController : ControllerBase
{
    /// <summary>
    /// Get all lotteries
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "Lotteries endpoint is working!" });
    }
}