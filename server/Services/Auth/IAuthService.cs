using Server.DTO.Auth;

namespace Server.Services.Auth;

public interface IAuthService
{
  Task<(bool Success, string Token, string Error)> LoginAsync(LoginRequestDTO request);
  Task<(bool Success, string Token, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDTO request);
}