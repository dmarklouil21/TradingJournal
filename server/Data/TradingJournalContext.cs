using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Server.Models;
using Server.Models.InvestingTracker;
using Server.Models.TradingJournal;
namespace Server.Data;

public class TradingJournalContext : IdentityDbContext<ApplicationUser>
{
  public TradingJournalContext(DbContextOptions<TradingJournalContext> options) : base (options)
  {
  }

  public DbSet<Assets> Assets { get; set; } = default!;
  public DbSet<DCACampaigns> DCACampaigns { get; set; } = default!;
  public DbSet<InvestmentLogs> InvestmentLogs { get; set; } = default!;

  public DbSet<TradingStrategies> TradingStrategies { get; set; } = default!;
  public DbSet<ActiveTrades> ActiveTrades { get; set; } = default!;
}