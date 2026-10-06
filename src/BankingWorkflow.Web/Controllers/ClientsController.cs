using BankingWorkflow.Application.DTOs.Clients;
using BankingWorkflow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingWorkflow.Web.Controllers;

[Authorize]
public class ClientsController : BaseApiController
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct)
    {
        var clients = await _clientService.GetAllAsync(search, ct);
        return Ok(clients);
    }

    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<ClientLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLookup([FromQuery] string? search, CancellationToken ct)
    {
        var lookup = await _clientService.GetLookupAsync(search, ct);
        return Ok(lookup);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var client = await _clientService.GetByIdAsync(id, ct);
        if (client == null)
        {
            return NotFound(new { message = $"Клиент с ID {id} не найден" });
        }

        return Ok(client);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateClientDto dto, CancellationToken ct)
    {
        var result = await _clientService.CreateAsync(dto, CurrentUserId, CurrentUserName, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _clientService.UpdateAsync(dto, CurrentUserId, CurrentUserName, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _clientService.DeleteAsync(id, CurrentUserId, CurrentUserName, ct);
        return NoContent();
    }
}
