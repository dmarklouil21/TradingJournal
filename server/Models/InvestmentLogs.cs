namespace Server.Models;

public class InvestmentLogs
{
  public int Id { get; set; }
  public int CampaignId { get; set; }
  public DCACampaigns? Campaign { get; set; }
  public DateTime ExecutionDate { get; set; }
  public decimal AmountTokens { get; set; } 
  public decimal PurchasePrice { get; set; }
  public decimal Fees { get; set; }
}