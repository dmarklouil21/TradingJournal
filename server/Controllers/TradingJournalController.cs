using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Server.DTO.TradingJournal;
using Server.Services.TradingJournal;

namespace Server.Controllers;

[Route("api/trading-journal")]
[ApiController]
[Authorize]
public class TradingJournalController : ControllerBase
{
  private readonly ITradingJournalService _tradingJournalService;

  public TradingJournalController(ITradingJournalService tradingJournalService)
  {
    _tradingJournalService = tradingJournalService;
  }

  [HttpPost("new-trade")]
  public async Task<IActionResult> AddTrade([FromBody] NewTradeDTO request)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if(string.IsNullOrEmpty(userId))
    {
      return Unauthorized(new { Error = "User id not found in token."});
    }

    var result = await _tradingJournalService.AddTradeAsync(userId, request);
    if(result.Success)
    {
      return Ok(new { Message = "Trade log successfully."});
    }

    return BadRequest(new { Error = result.Error });
  }

  [HttpGet("trades")]
  public async Task<IActionResult> GetTrades()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if(string.IsNullOrEmpty(userId))
    {
      return Unauthorized(new { Error = "User id not found in token."});
    }

    var trades = await _tradingJournalService.GetTradesAsync(userId);
    return Ok(trades);
  }
}