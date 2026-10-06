using BankingWorkflow.Application.DTOs.Applications;

namespace BankingWorkflow.Application.Interfaces;

public interface IExcelExportService
{
    Task<byte[]> ExportApplicationsToExcelAsync(
        ApplicationFilterDto filter,
        CancellationToken cancellationToken = default);
}
