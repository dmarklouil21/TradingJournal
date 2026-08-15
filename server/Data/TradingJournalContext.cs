using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Server.Models;
namespace Server.Data;

public class TradingJournalContext : IdentityDbContext<ApplicationUser>
{
  public TradingJournalContext(DbContextOptions<TradingJournalContext> options) : base (options)
  {
  }

  public DbSet<Assets> Assets { get; set; } = default!;
  public DbSet<DCACampaigns> DCACampaigns { get; set; } = default!;
  public DbSet<InvestmentLogs> InvestmentLogs { get; set; } = default!;
}