using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Server.Services.ExchangeRate;

public class ExchangeRateService : IExchangeRateService
{
  private readonly HttpClient _httpClient;
  private readonly IMemoryCache _cache;
  private readonly ILogger<ExchangeRateService> _logger;
  private const string CacheKey = "UsdToPhpRate";
  private const decimal FallbackRate = 56.00m; // Fallback hardcoded rate

  public ExchangeRateService(HttpClient httpClient, IMemoryCache cache, ILogger<ExchangeRateService> logger)
  {
    _httpClient = httpClient;
    _cache = cache;
    _logger = logger;
  }

  public async Task<decimal> GetUsdToPhpRateAsync()
  {
    if (_cache.TryGetValue(CacheKey, out decimal cachedRate))
    {
      return cachedRate;
    }

    try
    {
      // Using a free, no-key public API for exchange rates
      var response = await _httpClient.GetAsync("https://api.exchangerate-api.com/v4/latest/USD");
      if (response.IsSuccessStatusCode)
      {
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        
        if (doc.RootElement.TryGetProperty("rates", out var rates) && 
          rates.TryGetProperty("PHP", out var phpRateElement))
        {
          var rate = phpRateElement.GetDecimal();
          
          // Cache the rate for 6 hours
          var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(6));
              
          _cache.Set(CacheKey, rate, cacheOptions);
          return rate;
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to fetch live exchange rate. Using fallback.");
    }

    return FallbackRate;
  }

  public async Task<decimal> ConvertUsdToPhpAsync(decimal usdAmount)
  {
    var rate = await GetUsdToPhpRateAsync();
    return usdAmount * rate;
  }
}
