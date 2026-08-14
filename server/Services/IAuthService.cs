using System.Collections.Generic;
using System.Threading.Tasks;

using Server.DTO;

namespace Server.Services;

public interface IAuthService
{
  Task<(bool Success, string Token, string Error)> LoginAsync(LoginRequestDTO request);
  Task<(bool Success, string Token, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDTO request);
}