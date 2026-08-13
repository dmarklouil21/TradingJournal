using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

using Server.Models;
using Server.DTO;

namespace Server.Services;

public class AuthService : IAuthService
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;

  public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
  {
    _userManager = userManager;
    _signInManager = signInManager;
  }

  public async Task<(bool Success, string Error)> LoginAsync(LoginRequestDTO request)
  {
    var user = await _userManager.FindByNameAsync(request.Email);
    if(user == null) return (false, "Invalid login attempt.");

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
    if(result.Succeeded)
    {
      await _signInManager.SignInAsync(user, isPersistent: false);
      return (true, string.Empty);
    }

    return (false, "Invalid login attempt.");
  }

  public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDTO request)
  {
    var existingEmail = await _userManager.FindByEmailAsync(request.Email);
    if (existingEmail != null)
    {
      return (false, new [] {"An account with this email already exists."});
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
      return (true, Array.Empty<string>());
    }

    return (false, result.Errors.Select(e => e.Description));
  }
}