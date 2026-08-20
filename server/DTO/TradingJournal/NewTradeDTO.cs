using System.ComponentModel.DataAnnotations;
using Server.Models.TradingJournal.Enums;

namespace Server.DTO.TradingJournal;

public class NewTradeDTO
{
  [Required(ErrorMessage = "Instrument field is required")]
  public string Instrument { get; set; } = string.Empty;
  [Required(ErrorMessage = "Position type field is required")]
  public PositionType PositionType { get; set; }
  [Required(ErrorMessage = "Entry Date is required")]
  public DateTime EntryDate { get; set; }
  [Required(ErrorMessage = "Entry price is required")]
  public decimal EntryPrice { get; set; }
  [Required(ErrorMessage = "Position size is required")]
  public decimal PositionSize { get; set; }
  public DateTime? ExitDate { get; set; }
  public decimal? ExitPrice { get; set; }
  public decimal? RealizedPnL { get; set; }
  [Required(ErrorMessage = "Strategy is required")]
  public int StrategyId { get; set; }
  public string? ReviewNotes { get; set; }
}