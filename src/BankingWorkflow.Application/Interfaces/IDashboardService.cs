using BankingWorkflow.Application.DTOs.Dashboard;

namespace BankingWorkflow.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
