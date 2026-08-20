using Server.Models.TradingJournal.Enums;

namespace Server.Models.TradingJournal;

public class ActiveTrades
{
  public int Id { get; set; }
  public string Instrument { get; set; } = string.Empty;
  public int StrategyId { get; set; }
  public TradingStrategies? Strategy { get; set; }
  public PositionType PositionType{ get; set; }
  public DateTime EntryDate { get; set; }
  public decimal EntryPrice { get; set; }
  public decimal PositionSize { get; set; }
  public DateTime? ExitDate { get; set; }
  public decimal? ExitPrice { get; set; }
  public decimal TotalFees { get; set; } = 0;
  public decimal? RealizedPnL { get; set; }
  public string? ChartImageUrl { get; set; }
  public string? ReviewNotes { get; set; }
}