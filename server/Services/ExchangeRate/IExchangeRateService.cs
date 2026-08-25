using System.Threading.Tasks;

namespace Server.Services.ExchangeRate;

public interface IExchangeRateService
{
  Task<decimal> GetUsdToPhpRateAsync();
  Task<decimal> ConvertUsdToPhpAsync(decimal usdAmount);
}
