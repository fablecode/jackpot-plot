using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lottery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PinnedCombinationsController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PinnedCombinationsController(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    [HttpPost]
    public IActionResult Post([FromBody] PinCombinationRequest request)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Created(new Uri("http://won.com"), 1);
    }
}

public record PinCombinationRequest(long LotteryId, long PredictionId, int[] Numbers, DateTime PinnedDate);