using Server.DTO.TradingJournal;

namespace Server.Services.TradingJournal;

public interface ITradingJournalService
{
  Task<(bool Success, string Error)> AddTradeAsync(string userId, NewTradeDTO newTradeDTO, IFormFile? file);
  Task<List<ActiveTradeDTO>> GetTradesAsync(string userId);
}