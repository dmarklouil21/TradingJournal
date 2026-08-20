using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTO.TradingJournal;
using Server.Models.TradingJournal;

namespace Server.Services.TradingJournal;

public class TradingJournalService : ITradingJournalService
{
  TradingJournalContext _context;

  public TradingJournalService(TradingJournalContext tradingJournalContext)
  {
    _context = tradingJournalContext;
  }

  public async Task<(bool Success, string Error)> AddTradeAsync(string userId, NewTradeDTO newTradeDTO)
  {
    try
    {
      var newTrade = new ActiveTrades
      {
        UserId = userId,
        Instrument = newTradeDTO.Instrument,
        StrategyId = newTradeDTO.StrategyId,
        PositionType = newTradeDTO.PositionType,
        EntryDate = newTradeDTO.EntryDate.ToUniversalTime(),
        EntryPrice = newTradeDTO.EntryPrice,
        PositionSize = newTradeDTO.PositionSize,
        ExitDate = newTradeDTO.ExitDate?.ToUniversalTime(),
        ExitPrice = newTradeDTO.ExitPrice,
        RealizedPnL = newTradeDTO.RealizedPnL,
        ReviewNotes = newTradeDTO.ReviewNotes,
        ChartImageUrl = null
      };

      _context.ActiveTrades.Add(newTrade);
      await _context.SaveChangesAsync();
      return (true, string.Empty);
    }
    catch(Exception ex)
    {
      return (false, ex.Message);
    } 
  }

  public async Task<List<ActiveTradeDTO>> GetTradesAsync(string userId)
  {
    var trades = await _context.ActiveTrades
      .Include(t => t.Strategy)
      .Where(t => t.UserId == userId)
      .OrderByDescending(t => t.EntryDate)
      .ToListAsync();

    return trades.Select(t => new ActiveTradeDTO
    {
      Id = t.Id,
      Instrument = t.Instrument,
      PositionType = t.PositionType.ToString(),
      Date = t.EntryDate,
      Pnl = t.RealizedPnL,
      Strategy = t.Strategy?.Name ?? "Unknown",
      HasChart = !string.IsNullOrEmpty(t.ChartImageUrl),
      Status = !t.ExitDate.HasValue ? "Open" : (t.RealizedPnL > 0 ? "Win" : (t.RealizedPnL < 0 ? "Loss" : "Breakeven")),
      ReviewNotes = t.ReviewNotes
    }).ToList();
  }
}