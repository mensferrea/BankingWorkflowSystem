using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingWorkflow.Web.Controllers;

[Authorize]
public class ApplicationsController : BaseApiController
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] ApplicationFilterDto filter, CancellationToken ct)
    {
        var (items, totalCount) = await _applicationService.GetPagedAsync(filter, ct);
        return Ok(new
        {
            items,
            totalCount,
            pageNumber = filter.PageNumber,
            pageSize = filter.PageSize
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var application = await _applicationService.GetByIdAsync(id, ct);
        if (application == null)
        {
            return NotFound(new { message = $"Заявка с ID {id} не найдена" });
        }

        return Ok(application);
    }

    [HttpGet("by-client/{clientId:guid}")]
    [ProducesResponseType(typeof(List<ApplicationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClientId(Guid clientId, CancellationToken ct)
    {
        var applications = await _applicationService.GetByClientIdAsync(clientId, ct);
        return Ok(applications);
    }

    [HttpPost]
    [Authorize(Roles = "Operator,Manager,Admin")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateApplicationDto dto, CancellationToken ct)
    {
        var result = await _applicationService.CreateAsync(dto, CurrentUserId, CurrentUserName, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Operator,Manager,Admin")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApplicationDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _applicationService.UpdateAsync(dto, CurrentUserId, CurrentUserName, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto, CancellationToken ct)
    {
        var result = await _applicationService.ChangeStatusAsync(
            id,
            dto,
            CurrentUserId,
            CurrentUserName,
            CurrentUserRole,
            ct);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _applicationService.DeleteAsync(id, CurrentUserId, CurrentUserName, ct);
        return NoContent();
    }
}
