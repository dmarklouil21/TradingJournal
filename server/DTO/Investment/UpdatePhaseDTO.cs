using Server.Models.Enums;

namespace Server.DTO;

public class UpdatePhaseDTO
{
  public int CampaignId { get; set; }
  public SystemPhase NewPhase { get; set; }
}
