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
        .FirstOrDefaultAsync(c => c.UserId == userId && c.AssetId == asset.Id);

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
}