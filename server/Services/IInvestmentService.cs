using Server.DTO;

namespace Server.Services;

public interface IInvestmentService
{
  Task<(bool Success, string Error)> AddPurchaseAsync(string userId, NewInvestmentDTO newInvestmentRequest);
  Task<List<DCACampaignDTO>> GetCampaignsAsync(string userId);
}
