using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Domain.Enums;

namespace BankingWorkflow.Application.Interfaces;

public interface IApplicationService
{
    Task<(List<ApplicationSummaryDto> Items, int TotalCount)> GetPagedAsync(
        ApplicationFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<ApplicationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ApplicationSummaryDto>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<ApplicationDto> CreateAsync(
        CreateApplicationDto dto,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    Task<ApplicationDto> UpdateAsync(
        UpdateApplicationDto dto,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    Task<ApplicationDto> ChangeStatusAsync(
        Guid applicationId,
        ChangeStatusDto dto,
        string userId,
        string userName,
        UserRole userRole,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);
}
