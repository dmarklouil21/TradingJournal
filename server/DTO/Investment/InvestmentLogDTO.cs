namespace Server.DTO;

public class InvestmentLogDTO
{
  public DateTime ExecutionDate { get; set; }
  public decimal AmountTokens { get; set; }
  public decimal PurchasePrice { get; set; }
  public decimal Fees { get; set; }
}
