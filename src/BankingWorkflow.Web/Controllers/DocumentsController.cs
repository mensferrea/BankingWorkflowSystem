using BankingWorkflow.Application.DTOs.Documents;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingWorkflow.Web.Controllers;

[Authorize]
public class DocumentsController : BaseApiController
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("application/{applicationId:guid}")]
    [ProducesResponseType(typeof(List<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByApplication(Guid applicationId, CancellationToken ct)
    {
        var docs = await _documentService.GetByApplicationIdAsync(applicationId, ct);
        return Ok(docs);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Upload(
        [FromForm] Guid applicationId,
        [FromForm] DocumentType documentType,
        IFormFile file,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Файл не прикреплен или пуст" });
        }

        await using var stream = file.OpenReadStream();
        var uploadDto = new UploadDocumentDto
        {
            ApplicationId = applicationId,
            DocumentType = documentType,
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            Stream = stream,
            Length = file.Length
        };

        var result = await _documentService.UploadAsync(uploadDto, CurrentUserId, CurrentUserName, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var (stream, contentType, fileName) = await _documentService.DownloadAsync(id, ct);
        return File(stream, contentType, fileName);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _documentService.DeleteAsync(id, CurrentUserId, CurrentUserName, ct);
        return NoContent();
    }
}
