using System;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Server.Models;
using Server.Data;
using Server.Services;

namespace Server;

public class Program 
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Services.AddControllers();
    builder.Services.AddAuthorization();
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<TradingJournalContext>();
    
    builder.Services.AddCors(options =>
    {
      options.AddDefaultPolicy(policy =>
      {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
      });
    });

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<TradingJournalContext>(options => options.UseNpgsql(connectionString));

    builder.Services.AddScoped<IAuthService, AuthService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.MapOpenApi();
    }

    app.UseCors();
    
    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthorization();

    app.MapControllers();
    app.Run();
  }
}
