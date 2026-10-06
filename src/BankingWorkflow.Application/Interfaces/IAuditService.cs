using BankingWorkflow.Application.DTOs.Audit;

namespace BankingWorkflow.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string? userId,
        string userName,
        string action,
        string entityName,
        string? entityId,
        string details,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<(List<AuditLogDto> Items, int TotalCount)> GetPagedAsync(
        AuditLogFilterDto filter,
        CancellationToken cancellationToken = default);
}
