using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services.Dashboard;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
  private readonly IDashboardService _dashboardService;

  public DashboardController(IDashboardService dashboardService)
  {
    _dashboardService = dashboardService;
  }

  [HttpGet("summary")]
  public async Task<IActionResult> GetSummary()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
      return Unauthorized(new { Error = "User ID not found in token." });
    }

    var summary = await _dashboardService.GetSummaryAsync(userId);
    return Ok(summary);
  }
}
