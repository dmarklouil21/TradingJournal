using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Server.Models;
using Server.Data;
using Server.Services.Auth;
using Server.Services.InvestingTracker;
using Server.Services.Settings;
using Server.Services.TradingJournal;

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

    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<TradingJournalContext>();

    builder.Services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
      };
    });

    builder.Services.AddAuthorization();
    
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
    builder.Services.AddScoped<IInvestmentService, InvestmentService>();
    builder.Services.AddScoped<IActiveTradingSettingsService, ActiveTradingSettingsService>();
    builder.Services.AddScoped<ITradingJournalService, TradingJournalService>();
    
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.MapOpenApi();
    }

    app.UseCors();
    
    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.Run();
  }
}
