using Server.Models.Enums;

namespace Server.Models;

public class DCACampaigns
{
  public int Id { get; set; }
  public string UserId { get; set; } = string.Empty;
  public ApplicationUser? User { get; set; }
  
  public int AssetId { get; set; }
  public Assets? Asset { get; set; }

  public Status Status { get; set; }
  public SystemPhase SystemPhase { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}