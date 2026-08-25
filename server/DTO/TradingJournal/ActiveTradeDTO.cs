using System;

namespace Server.DTO.TradingJournal;

public class ActiveTradeDTO
{
  public int Id { get; set; }
  public string Instrument { get; set; } = string.Empty;
  public string PositionType { get; set; } = string.Empty;
  public DateTime Date { get; set; }
  public decimal? Pnl { get; set; }
  public string Strategy { get; set; } = string.Empty;
  public bool HasChart { get; set; }
  public string Status { get; set; } = string.Empty;
  public string? ReviewNotes { get; set; }
  
  // Detailed fields
  public decimal EntryPrice { get; set; }
  public decimal? ExitPrice { get; set; }
  public decimal PositionSize { get; set; }
  public DateTime? ExitDate { get; set; }
  public string? ChartImageUrl { get; set; }
}
