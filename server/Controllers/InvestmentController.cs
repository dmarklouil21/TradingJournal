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

  [HttpGet("campaigns")]
  public async Task<IActionResult> GetCampaigns()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
      return Unauthorized(new { Error = "User ID not found in token." });
    }

    var result = await _investmentService.GetCampaignsAsync(userId);
    return Ok(result);
  }

  [HttpPost("sale")]
  public async Task<IActionResult> LogSale([FromBody] LogSaleDTO request)
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Error = "User ID not found in token." });

    var result = await _investmentService.LogSaleAsync(userId, request);
    
    if (result.Success) return Ok(new { Message = "Sale logged successfully" });
    return BadRequest(new { Error = result.Error });
  }

  [HttpPost("phase")]
  public async Task<IActionResult> UpdatePhase([FromBody] UpdatePhaseDTO request)
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Error = "User ID not found in token." });

    var result = await _investmentService.UpdatePhaseAsync(userId, request);
    
    if (result.Success) return Ok(new { Message = "Phase updated successfully" });
    return BadRequest(new { Error = result.Error });
  }

  [HttpGet("price/{symbol}")]
  public async Task<IActionResult> GetLivePrice(string symbol)
  {
    try 
    {
      using var client = new System.Net.Http.HttpClient();
      var url = $"https://api.pro.coins.ph/openapi/quote/v1/ticker/price?symbol={symbol.ToUpper()}";
      var response = await client.GetAsync(url);
      
      if (!response.IsSuccessStatusCode)
        return BadRequest(new { Error = "Failed to fetch price from Coins.ph" });

      var content = await response.Content.ReadAsStringAsync();
      return Content(content, "application/json");
    }
    catch (Exception ex)
    {
      return StatusCode(500, new { Error = ex.Message });
    }
  }

  private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte[]> _iconCache = new();

  [AllowAnonymous]
  [HttpGet("logo/{symbol}")]
  public async Task<IActionResult> GetCryptoLogo(string symbol)
  {
    var key = symbol.ToLower();
    
    // Serve from server RAM immediately if it's downloaded once
    if (_iconCache.TryGetValue(key, out var cachedSvg))
    {
      return File(cachedSvg, "image/svg+xml");
    }

    try 
    {
      using var client = new System.Net.Http.HttpClient();
      client.Timeout = TimeSpan.FromSeconds(5);
      client.DefaultRequestHeaders.Add("User-Agent", "TradingJournal/1.0");
      
      var url = $"https://cdn.jsdelivr.net/npm/cryptocurrency-icons/svg/color/{key}.svg";
      var response = await client.GetAsync(url);
      
      if (!response.IsSuccessStatusCode)
        return NotFound(new { Error = "Logo not found" });

      var svgBytes = await response.Content.ReadAsByteArrayAsync();
      
      // Cache it forever in RAM
      _iconCache[key] = svgBytes;

      return File(svgBytes, "image/svg+xml");
    }
    catch
    {
      return NotFound();
    }
  }
}
