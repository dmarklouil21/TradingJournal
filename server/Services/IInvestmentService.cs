using Server.DTO;

namespace Server.Services;

public interface IInvestmentService
{
  Task<(bool Success, string Error)> AddPurchaseAsync(string userId, NewInvestmentDTO newInvestmentRequest);
  Task<(bool Success, string Error)> LogSaleAsync(string userId, LogSaleDTO request);
  Task<(bool Success, string Error)> UpdatePhaseAsync(string userId, UpdatePhaseDTO request);
  Task<List<DCACampaignDTO>> GetCampaignsAsync(string userId);
}
