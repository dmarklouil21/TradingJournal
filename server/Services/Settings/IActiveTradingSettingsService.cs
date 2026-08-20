using System.Collections.Generic;
using System.Threading.Tasks;
using Server.DTO.Settings;
using Server.Models.TradingJournal;

namespace Server.Services.Settings;

public interface IActiveTradingSettingsService
{
  Task<(bool Success, string? Error, TradingStrategies? Strategy)> AddStrategyAsync(string userId, NewStrategyDTO request);
  Task<List<TradingStrategies>> GetStrategiesAsync(string userId);
}
