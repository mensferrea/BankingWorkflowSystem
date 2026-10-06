using BankingWorkflow.Domain.Enums;

namespace BankingWorkflow.Application.DTOs.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FormattedSize => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} B",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
        _ => $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB"
    };
    public DateTimeOffset UploadedAt { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
}

public class UploadDocumentDto
{
    public Guid ApplicationId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Stream Stream { get; set; } = Stream.Null;
    public long Length { get; set; }
}
