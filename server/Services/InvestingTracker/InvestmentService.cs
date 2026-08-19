using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTO.InvestingTracker;
using Server.Models.InvestingTracker;
using Server.Models.InvestingTracker.Enums;

namespace Server.Services.InvestingTracker;

public class InvestmentService : IInvestmentService
{
  private readonly TradingJournalContext _context;

  public InvestmentService(TradingJournalContext tradingJournalContext)
  {
    _context = tradingJournalContext;
  }
  
  public async Task<(bool Success, string Error)> AddPurchaseAsync(string userId, NewInvestmentDTO request)
  {
    try 
    {
      var asset = await _context.Assets
        .FirstOrDefaultAsync(a => a.Symbol.ToLower() == request.Symbol.ToLower());

      if (asset == null)
      {
        asset = new Assets
        {
          Symbol = request.Symbol.ToUpper(),
          Name = request.Name,
          AssetType = AssetType.Crypto 
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
      }

      var campaign = await _context.DCACampaigns
        .FirstOrDefaultAsync(c => c.UserId == userId && c.AssetId == asset.Id && c.Status == Status.Active);

      if (campaign == null)
      {
        campaign = new DCACampaigns
        {
          UserId = userId,
          AssetId = asset.Id,
          Status = Status.Active,
          SystemPhase = SystemPhase.PhaseOne
        };
        _context.DCACampaigns.Add(campaign);
        await _context.SaveChangesAsync();
      }

      var log = new InvestmentLogs
      {
        CampaignId = campaign.Id,
        ExecutionDate = request.ExecutionDate.ToUniversalTime(),
        AmountTokens = request.AmountTokens,
        PurchasePrice = request.PurchasePrice,
        Fees = request.Fees
      };

      _context.InvestmentLogs.Add(log);
      await _context.SaveChangesAsync();

      return (true, string.Empty);
    }
    catch (Exception ex)
    {
      return (false, ex.Message);
    }
  }

  public async Task<List<DCACampaignDTO>> GetCampaignsAsync(string userId)
  {
    var campaigns = await _context.DCACampaigns
      .Include(c => c.Asset)
      .Where(c => c.UserId == userId && c.Status == Status.Active) 
      .Select(c => new 
      {
        Id = c.Id,
        Asset = c.Asset.Name,
        Symbol = c.Asset.Symbol,
        Phase = c.SystemPhase,
        Logs = _context.InvestmentLogs.Where(l => l.CampaignId == c.Id).ToList()
      })
      .ToListAsync();

    var dtoList = campaigns
      .Select(c => 
      {
        decimal currentHoldings = 0;
        decimal currentTotalCost = 0;

        // Calculate Rolling Standard Average Cost Basis
        foreach (var log in c.Logs.OrderBy(l => l.ExecutionDate))
        {
          if (log.AmountTokens > 0)
          {
            // Purchase adds to total cost
            currentTotalCost += (log.AmountTokens * log.PurchasePrice) + log.Fees;
            currentHoldings += log.AmountTokens;
          }
          else
          {
            // Sale proportionally removes from total cost based on the CURRENT average cost
            var avgCostAtSale = currentHoldings > 0 ? currentTotalCost / currentHoldings : 0;
            
            // AmountTokens is negative, so we add it to subtract
            currentHoldings += log.AmountTokens;
            
            // Subtract the exact cost basis of the tokens sold (ignoring sale proceeds for Avg Cost)
            currentTotalCost += (log.AmountTokens * avgCostAtSale);
          }
        }

        var finalAvgCost = currentHoldings > 0 ? currentTotalCost / currentHoldings : 0;

        return new 
        {
          Data = c,
          TotalHoldings = currentHoldings,
          AvgCost = finalAvgCost
        };
      })
      .Where(x => x.TotalHoldings > 0) 
      .Select(x => new DCACampaignDTO
      {
        Id = x.Data.Id,
        Asset = x.Data.Asset,
        Symbol = x.Data.Symbol,
        Phase = x.Data.Phase.ToString(), 
        Holdings = x.TotalHoldings.ToString("0.######"), 
        AvgCost = x.AvgCost,
        CurrentPrice = 0,
        Logs = x.Data.Logs.Select(l => new InvestmentLogDTO
        {
          ExecutionDate = l.ExecutionDate,
          AmountTokens = l.AmountTokens,
          PurchasePrice = l.PurchasePrice,
          Fees = l.Fees
        }).ToList()
      }).ToList();

    return dtoList;
  }

  public async Task<(bool Success, string Error)> LogSaleAsync(string userId, LogSaleDTO request)
  {
    try 
    {
      var campaign = await _context.DCACampaigns.FirstOrDefaultAsync(c => c.Id == request.CampaignId && c.UserId == userId);
      if (campaign == null) return (false, "Campaign not found.");

      var currentHoldings = await _context.InvestmentLogs
        .Where(l => l.CampaignId == campaign.Id)
        .SumAsync(l => l.AmountTokens);

      if (currentHoldings < request.AmountTokens)
      {
        return (false, "Insufficient holdings for this sale.");
      }

      var log = new InvestmentLogs
      {
        CampaignId = campaign.Id,
        ExecutionDate = request.ExecutionDate.ToUniversalTime(),
        AmountTokens = -request.AmountTokens, 
        PurchasePrice = request.SellPrice,
        Fees = request.Fees 
      };

      _context.InvestmentLogs.Add(log);

      if (currentHoldings - request.AmountTokens == 0)
      {
        campaign.Status = Status.Completed;
      }

      await _context.SaveChangesAsync();

      return (true, string.Empty);
    }
    catch (Exception ex)
    {
      return (false, ex.Message);
    }
  }

  public async Task<(bool Success, string Error)> UpdatePhaseAsync(string userId, UpdatePhaseDTO request)
  {
    try
    {
      var campaign = await _context.DCACampaigns.FirstOrDefaultAsync(c => c.Id == request.CampaignId && c.UserId == userId);
      if (campaign == null) return (false, "Campaign not found.");

      campaign.SystemPhase = request.NewPhase;
      await _context.SaveChangesAsync();
      
      return (true, string.Empty);
    }
    catch (Exception ex)
    {
      return (false, ex.Message);
    }
  }
}