using Microsoft.AspNetCore.Mvc;

using Server.DTO;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;

  public AuthController(IAuthService authService)
  {
    _authService = authService;
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
  {
    var result = await _authService.LoginAsync(request);
    if(!result.Success)
      return Unauthorized(new { Error = result.Error });

    var response = new LoginResponseDTO
    {
      Token = result.Token,
      Username = request.Email,
      Expiration = DateTime.UtcNow.AddHours(1)
    };

    return Ok(response);
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
  {
    var result = await _authService.RegisterAsync(request);

    if(!result.Success) 
      return BadRequest(new { Errors = result.Errors });

    var response = new LoginResponseDTO
    {
      Token = result.Token,
      Username = request.Email,
      Expiration = DateTime.UtcNow.AddHours(1)
    };

    return Ok(response);
  }
}