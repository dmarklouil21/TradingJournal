using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Server.DTO.Settings;
using Server.Services.Settings;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SettingsController : ControllerBase
{
  private readonly IActiveTradingSettingsService _activeTradingSettingsService;

  public SettingsController(IActiveTradingSettingsService activeTradingSettingsService)
  {
    _activeTradingSettingsService = activeTradingSettingsService;
  }

  [HttpPost("strategy")]
  public async Task<IActionResult> AddStrategy([FromBody] NewStrategyDTO request)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
      return Unauthorized(new { Error = "User ID not found in token." });
    }

    var result = await _activeTradingSettingsService.AddStrategyAsync(userId, request);
    
    if (result.Success)
    {
      return Ok(result.Strategy);
    }

    return BadRequest(new { Error = result.Error });
  }

  [HttpGet("strategies")]
  public async Task<IActionResult> GetStrategies()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
      return Unauthorized(new { Error = "User ID not found in token." });
    }

    var strategies = await _activeTradingSettingsService.GetStrategiesAsync(userId);
    return Ok(strategies);
  }
}
