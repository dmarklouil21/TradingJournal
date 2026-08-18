using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTO;
using Server.Models;
using Server.Models.Enums;

namespace Server.Services;

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
        var totalHoldings = c.Logs.Sum(l => l.AmountTokens);
        var totalCost = c.Logs.Sum(l => (l.AmountTokens * l.PurchasePrice) + l.Fees);
        var avgCost = totalHoldings > 0 ? totalCost / totalHoldings : 0;

        return new 
        {
          Data = c,
          TotalHoldings = totalHoldings,
          AvgCost = avgCost
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