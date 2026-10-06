using BankingWorkflow.Application.DTOs.Audit;
using BankingWorkflow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingWorkflow.Web.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter, CancellationToken ct)
    {
        var (items, totalCount) = await _auditService.GetPagedAsync(filter, ct);
        return Ok(new
        {
            items,
            totalCount,
            pageNumber = filter.PageNumber,
            pageSize = filter.PageSize
        });
    }
}
