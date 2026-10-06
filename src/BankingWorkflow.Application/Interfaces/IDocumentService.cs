using BankingWorkflow.Application.DTOs.Documents;

namespace BankingWorkflow.Application.Interfaces;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<DocumentDto> UploadAsync(UploadDocumentDto dto, string userId, string userName, CancellationToken cancellationToken = default);
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid documentId, string userId, string userName, CancellationToken cancellationToken = default);
}
