using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTO.Dashboard;
using Server.Models.TradingJournal;
using Server.Models.InvestingTracker;
using Server.Services.ExchangeRate;

namespace Server.Services.Dashboard;

public class DashboardService : IDashboardService
{
  private readonly TradingJournalContext _context;
  private readonly IExchangeRateService _exchangeRateService;

  public DashboardService(TradingJournalContext context, IExchangeRateService exchangeRateService)
  {
    _context = context;
    _exchangeRateService = exchangeRateService;
  }

  public async Task<DashboardSummaryDTO> GetSummaryAsync(string userId)
  {
    var investmentLogs = await _context.InvestmentLogs
      .Include(l => l.Campaign)
      .ThenInclude(c => c.Asset)
      .Where(l => l.Campaign != null && l.Campaign.UserId == userId)
      .ToListAsync();
        
    var totalInvestedPhp = investmentLogs.Sum(l => (l.AmountTokens * l.PurchasePrice) + l.Fees);

    var activeTrades = await _context.ActiveTrades
      .Where(t => t.UserId == userId)
      .ToListAsync();
        
    var totalPnLUsd = activeTrades.Sum(t => t.RealizedPnL);
    
    // Convert PnL from USD to PHP using live exchange rate
    var totalPnLPhp = await _exchangeRateService.ConvertUsdToPhpAsync(totalPnLUsd);
    
    var totalNetWorth = totalInvestedPhp + totalPnLPhp;

    var activities = new List<RecentActivityDTO>();

    foreach (var log in investmentLogs)
    {
      activities.Add(new RecentActivityDTO
      {
        Id = log.Id * 10, // Avoid ID collisions
        Type = "dca",
        Action = log.AmountTokens > 0 ? "Bought" : "Sold",
        Asset = log.Campaign?.Asset?.Name ?? "Asset",
        Amount = $"-₱{log.AmountTokens * log.PurchasePrice + log.Fees:F2}",
        Date = log.ExecutionDate,
        Icon = "M12 6v6m0 0v6m0-6h6m-6 0H6"
      });
    }

    foreach (var trade in activeTrades)
    {
      var isWin = trade.RealizedPnL > 0;
      var actionText = isWin ? "Closed Win" : "Closed Loss";
      var icon = isWin ? "M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" : "M6 18L18 6M6 6l12 12";
      var amountStr = $"{(isWin ? "+" : "")}${trade.RealizedPnL:F2}";

      activities.Add(new RecentActivityDTO
      {
        Id = (trade.Id * 10) + 1,
        Type = "active",
        Action = actionText,
        Asset = trade.Instrument,
        Amount = amountStr,
        Date = trade.EntryDate,
        Icon = icon
      });
    }

    return new DashboardSummaryDTO
    {
      TotalNetWorth = totalNetWorth,
      TrendPercentage = 0, // Mock for now
      RecentActivities = activities.OrderByDescending(a => a.Date).Take(10).ToList()
    };
  }
}
