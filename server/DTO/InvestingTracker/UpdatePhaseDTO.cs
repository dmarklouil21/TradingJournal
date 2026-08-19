using Server.Models.InvestingTracker.Enums;

namespace Server.DTO.InvestingTracker;

public class UpdatePhaseDTO
{
  public int CampaignId { get; set; }
  public SystemPhase NewPhase { get; set; }
}
