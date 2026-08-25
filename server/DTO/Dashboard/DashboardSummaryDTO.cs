using System;
using System.Collections.Generic;

namespace Server.DTO.Dashboard;

public class DashboardSummaryDTO
{
  public decimal TotalNetWorth { get; set; }
  public decimal TrendPercentage { get; set; }
  public List<RecentActivityDTO> RecentActivities { get; set; } = new();
}

public class RecentActivityDTO
{
  public int Id { get; set; }
  public string Type { get; set; } = string.Empty;
  public string Action { get; set; } = string.Empty;
  public string Asset { get; set; } = string.Empty;
  public string Amount { get; set; } = string.Empty;
  public DateTime Date { get; set; }
  public string Icon { get; set; } = string.Empty;
}
