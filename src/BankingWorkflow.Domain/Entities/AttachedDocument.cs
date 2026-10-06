using BankingWorkflow.Domain.Enums;

namespace BankingWorkflow.Domain.Entities;

public class AttachedDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public Application? Application { get; set; }

    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    public string UploadedByUserId { get; set; } = string.Empty;
    public string UploadedByUserName { get; set; } = string.Empty;
}
