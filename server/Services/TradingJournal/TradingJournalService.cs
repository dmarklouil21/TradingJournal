using Microsoft.EntityFrameworkCore;

using Supabase.Storage;

using Server.Data;
using Server.DTO.TradingJournal;
using Server.Models.TradingJournal;

namespace Server.Services.TradingJournal;

public class TradingJournalService : ITradingJournalService
{
  TradingJournalContext _context;
  private readonly Supabase.Client _supabase;

  public TradingJournalService(TradingJournalContext tradingJournalContext, Supabase.Client supabase)
  {
    _context = tradingJournalContext;
    _supabase = supabase;
  }

  public async Task<(bool Success, string Error)> AddTradeAsync(string userId, NewTradeDTO newTradeDTO, IFormFile? file)
  {
    try
    {
      string? chartImageUrl = null; 

      if(!(file == null || file.Length == 0))
      {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

        var supabasePath = $"uploads/{uniqueFileName}";
      
        try
        {
          await _supabase.Storage
            .From("images")
            .Upload(fileBytes, supabasePath, new Supabase.Storage.FileOptions
            {
              CacheControl = "3600",
              Upsert = false
            });
          
          chartImageUrl = _supabase.Storage
            .From("images")
            .GetPublicUrl(supabasePath);
        }
        catch(Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }

      var newTrade = new ActiveTrades
      {
        UserId = userId,
        Instrument = newTradeDTO.Instrument,
        StrategyId = newTradeDTO.StrategyId,
        PositionType = newTradeDTO.PositionType,
        EntryDate = newTradeDTO.EntryDate.ToUniversalTime(),
        EntryPrice = newTradeDTO.EntryPrice,
        PositionSize = newTradeDTO.PositionSize,
        ExitDate = newTradeDTO.ExitDate.ToUniversalTime(),
        ExitPrice = newTradeDTO.ExitPrice,
        RealizedPnL = newTradeDTO.RealizedPnL,
        ReviewNotes = newTradeDTO.ReviewNotes,
        ChartImageUrl = chartImageUrl
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
      Status = t.RealizedPnL > 0 ? "Win" : (t.RealizedPnL < 0 ? "Loss" : "Breakeven"),
      ReviewNotes = t.ReviewNotes,
      EntryPrice = t.EntryPrice,
      ExitPrice = t.ExitPrice,
      PositionSize = t.PositionSize,
      ExitDate = t.ExitDate,
      ChartImageUrl = t.ChartImageUrl
    }).ToList();
  }
}