namespace Server.DTO;

public class LogSaleDTO
{
  public int CampaignId { get; set; }
  public decimal AmountTokens { get; set; }
  public decimal SellPrice { get; set; }
  public decimal Fees { get; set; }
  public DateTime ExecutionDate { get; set; }
}
