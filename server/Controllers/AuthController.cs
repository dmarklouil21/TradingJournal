using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Server.DTO;
using Server.Services;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
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
    if(string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
      return BadRequest("Email and password are required");
    }

    var result = await _authService.LoginAsync(request);
    if(!result.Success)
    {
      return Unauthorized("Invalid login attempt.");
    }

    string sampleToken = "QwErTy"; // TODO: Implement real JWT generation

    var response = new LoginResponseDTO
    {
      Token = sampleToken,
      Username = request.Email,
      Expiration = DateTime.UtcNow.AddHours(1)
    };

    return Ok(response);
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
  {
    var result = await _authService.RegisterAsync(request);

    if(result.Success)
    {
      return Ok();
    }

    return BadRequest(new { Errors = result.Errors });
  }
}