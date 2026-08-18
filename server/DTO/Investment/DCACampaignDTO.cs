namespace Server.DTO;

public class DCACampaignDTO
{
  public int Id { get; set; }
  public string Asset { get; set; } = string.Empty;
  public string Symbol { get; set; } = string.Empty;
  public string Holdings { get; set; } = string.Empty;
  public decimal AvgCost { get; set; }
  public decimal CurrentPrice { get; set; }
  public string Phase { get; set; } = string.Empty;
  public List<InvestmentLogDTO> Logs { get; set; } = new();
}
