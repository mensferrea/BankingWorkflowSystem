using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingWorkflow.Web.Controllers;

[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IExcelExportService _excelService;
    private readonly IDocumentGenerationService _docGenService;
    private readonly IApplicationService _appService;

    public ReportsController(
        IExcelExportService excelService,
        IDocumentGenerationService docGenService,
        IApplicationService appService)
    {
        _excelService = excelService;
        _docGenService = docGenService;
        _appService = appService;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] ApplicationFilterDto filter, CancellationToken ct)
    {
        var bytes = await _excelService.ExportApplicationsToExcelAsync(filter, ct);
        var fileName = $"Applications_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("agreement/{applicationId:guid}")]
    public async Task<IActionResult> DownloadLoanAgreement(Guid applicationId, CancellationToken ct)
    {
        var app = await _appService.GetByIdAsync(applicationId, ct);
        if (app == null)
        {
            return NotFound(new { message = "Заявка не найдена" });
        }

        var bytes = await _docGenService.GenerateLoanAgreementDocxAsync(applicationId, ct);
        var fileName = $"Loan_Agreement_{app.ApplicationNumber}.docx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }

    [HttpGet("decision/{applicationId:guid}")]
    public async Task<IActionResult> DownloadDecisionCertificate(Guid applicationId, CancellationToken ct)
    {
        var app = await _appService.GetByIdAsync(applicationId, ct);
        if (app == null)
        {
            return NotFound(new { message = "Заявка не найдена" });
        }

        var bytes = await _docGenService.GenerateDecisionCertificateDocxAsync(applicationId, ct);
        var fileName = $"Decision_{app.ApplicationNumber}.docx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }
}
