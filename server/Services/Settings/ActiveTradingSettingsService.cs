using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTO.Settings;
using Server.Models.TradingJournal;

namespace Server.Services.Settings;

public class ActiveTradingSettingsService : IActiveTradingSettingsService
{
  private readonly TradingJournalContext _context;

  public ActiveTradingSettingsService(TradingJournalContext context)
  {
    _context = context;
  }

  public async Task<(bool Success, string? Error, TradingStrategies? Strategy)> AddStrategyAsync(string userId, NewStrategyDTO request)
  {
    try
    {
      // Check if user already has a strategy with this name
      var exists = await _context.TradingStrategies
        .AnyAsync(s => s.UserId == userId && s.Name.ToLower() == request.Name.ToLower());
        
      if (exists)
      {
        return (false, "A strategy with this name already exists.", null);
      }

      var newStrategy = new TradingStrategies
      {
        UserId = userId,
        Name = request.Name,
        Description = request.Description
      };

      _context.TradingStrategies.Add(newStrategy);
      await _context.SaveChangesAsync();

      return (true, null, newStrategy);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error adding strategy: {ex.Message}");
      return (false, "An error occurred while saving the strategy.", null);
    }
  }

  public async Task<List<TradingStrategies>> GetStrategiesAsync(string userId)
  {
    return await _context.TradingStrategies
      .Where(s => s.UserId == userId)
      .OrderBy(s => s.Name)
      .ToListAsync();
  }
}
