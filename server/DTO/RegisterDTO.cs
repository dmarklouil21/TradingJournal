using System;

namespace Server.DTO;

public class RegisterRequestDTO
{
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}
