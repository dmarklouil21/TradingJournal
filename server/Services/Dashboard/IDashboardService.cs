using System.Threading.Tasks;
using Server.DTO.Dashboard;

namespace Server.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummaryDTO> GetSummaryAsync(string userId);
}
