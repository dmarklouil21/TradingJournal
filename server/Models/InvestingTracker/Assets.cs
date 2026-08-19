using Server.Models.InvestingTracker.Enums;

namespace Server.Models.InvestingTracker;

public class Assets
{
  public int Id { get; set; }
  public string Symbol { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public AssetType AssetType { get; set; }
}