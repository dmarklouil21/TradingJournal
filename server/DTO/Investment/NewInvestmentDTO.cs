using System.ComponentModel.DataAnnotations;

namespace Server.DTO;

public class NewInvestmentDTO
{
  [Required(ErrorMessage = "Asset Name is required")]
  public string Name { get; set; } = string.Empty; 
  [Required(ErrorMessage = "Asset Symbol is required")]
  public string Symbol { get; set; } = string.Empty;
  [Required(ErrorMessage = "Amount Tokens is required")]
  public decimal AmountTokens { get; set; }
  [Required(ErrorMessage = "Purchase Price")]
  public decimal PurchasePrice { get; set; }
  [Required(ErrorMessage = "Fees is required")]
  public decimal Fees { get; set; }
  public DateTime ExecutionDate { get; set; }
}