using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Server.Models;
using Server.DTO;

namespace Server.Services;

public class AuthService : IAuthService
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly IConfiguration _configuration;

  public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _configuration = configuration;
  }

  public async Task<(bool Success, string Token, string Error)> LoginAsync(LoginRequestDTO request)
  {
    var user = await _userManager.FindByNameAsync(request.Email);
    if(user == null) 
      return (false, string.Empty, "Invalid login attempt.");

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
    if(result.Succeeded)
    {
      await _signInManager.SignInAsync(user, isPersistent: false);
      var token = GenerateJwtToken(user);
      return (true, token, string.Empty);
    }

    return (false, string.Empty, "Invalid login attempt.");
  }

  public async Task<(bool Success, string Token, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDTO request)
  {
    var existingEmail = await _userManager.FindByEmailAsync(request.Email);
    if (existingEmail != null)
    {
      return (false, string.Empty, new [] {"An account with this email already exists."});
    }
    
    var user = new ApplicationUser
    {
      FullName = request.FullName,
      UserName = request.Email,
      Email = request.Email
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if(result.Succeeded)
    {
      var token = GenerateJwtToken(user);
      return (true, token, Array.Empty<string>());
    }
    return (false, string.Empty, result.Errors.Select(e => e.Description));
  }

  private string GenerateJwtToken(ApplicationUser user)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
      new Claim(JwtRegisteredClaimNames.Email, user.Email!),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new Claim(ClaimTypes.NameIdentifier, user.Id)
    };

    var token = new JwtSecurityToken(
      issuer: _configuration["Jwt:Issuer"],
      audience: _configuration["Jwt:Audience"],
      claims: claims,
      expires: DateTime.UtcNow.AddHours(1),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}