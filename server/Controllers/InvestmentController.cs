using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Server.DTO;
using Server.Services;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InvestmentController : ControllerBase
{
  private readonly IInvestmentService _investmentService;

  public InvestmentController(IInvestmentService investmentService)
  {
    _investmentService = investmentService;
  }

  [HttpPost("purchase")]
  public async Task<IActionResult> LogPurchase([FromBody] NewInvestmentDTO request)
  {
    Console.WriteLine("Was I here?");
    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
      Console.WriteLine("User ID not found?");
      return Unauthorized(new { Error = "User ID not found in token." });
    }

    var result = await _investmentService.AddPurchaseAsync(userId, request);
    
    if (result.Success)
    {
      return Ok(new { Message = "Purchase logged successfully." });
    }

    return BadRequest(new { Error = result.Error });
  }
}
