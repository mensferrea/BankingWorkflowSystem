using BankingWorkflow.Application.DTOs.Documents;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Exceptions;
using BankingWorkflow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingWorkflow.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly BankingDbContext _db;
    private readonly IFileStorage _storage;
    private readonly IAuditService _auditService;

    public DocumentService(
        BankingDbContext db,
        IFileStorage storage,
        IAuditService auditService)
    {
        _db = db;
        _storage = storage;
        _auditService = auditService;
    }

    public async Task<List<DocumentDto>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _db.AttachedDocuments
            .AsNoTracking()
            .Where(d => d.ApplicationId == applicationId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                ApplicationId = d.ApplicationId,
                DocumentType = d.DocumentType,
                DocumentTypeName = d.DocumentType.ToString(),
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSizeBytes = d.FileSizeBytes,
                UploadedAt = d.UploadedAt,
                UploadedByUserName = d.UploadedByUserName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentDto> UploadAsync(
        UploadDocumentDto dto,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var application = await _db.Applications.FindAsync(new object[] { dto.ApplicationId }, cancellationToken);
        if (application == null)
        {
            throw new DomainException($"Заявка с ID '{dto.ApplicationId}' не найдена.");
        }

        var relativePath = await _storage.SaveAsync(dto.Stream, dto.FileName, cancellationToken);

        var doc = new AttachedDocument
        {
            ApplicationId = dto.ApplicationId,
            DocumentType = dto.DocumentType,
            FileName = dto.FileName,
            FilePath = relativePath,
            ContentType = string.IsNullOrWhiteSpace(dto.ContentType) ? "application/octet-stream" : dto.ContentType,
            FileSizeBytes = dto.Length,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadedByUserId = userId,
            UploadedByUserName = userName
        };

        _db.AttachedDocuments.Add(doc);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "UPLOAD_DOCUMENT",
            "Document",
            doc.Id.ToString(),
            $"Загружен документ '{doc.FileName}' к заявке № {application.ApplicationNumber}",
            cancellationToken: cancellationToken);

        return new DocumentDto
        {
            Id = doc.Id,
            ApplicationId = doc.ApplicationId,
            DocumentType = doc.DocumentType,
            DocumentTypeName = doc.DocumentType.ToString(),
            FileName = doc.FileName,
            ContentType = doc.ContentType,
            FileSizeBytes = doc.FileSizeBytes,
            UploadedAt = doc.UploadedAt,
            UploadedByUserName = doc.UploadedByUserName
        };
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var doc = await _db.AttachedDocuments.FindAsync(new object[] { documentId }, cancellationToken);
        if (doc == null)
        {
            throw new DomainException("Документ не найден.");
        }

        var stream = await _storage.OpenReadAsync(doc.FilePath, cancellationToken);
        return (stream, doc.ContentType, doc.FileName);
    }

    public async Task DeleteAsync(Guid documentId, string userId, string userName, CancellationToken cancellationToken = default)
    {
        var doc = await _db.AttachedDocuments
            .Include(d => d.Application)
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (doc == null) return;

        await _storage.DeleteAsync(doc.FilePath, cancellationToken);
        _db.AttachedDocuments.Remove(doc);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "DELETE_DOCUMENT",
            "Document",
            documentId.ToString(),
            $"Удален документ '{doc.FileName}'",
            cancellationToken: cancellationToken);
    }
}
