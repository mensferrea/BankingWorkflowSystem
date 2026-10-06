using BankingWorkflow.Application.DTOs.Clients;

namespace BankingWorkflow.Application.Interfaces;

public interface IClientService
{
    Task<List<ClientDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<List<ClientLookupDto>> GetLookupAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClientDto> CreateAsync(CreateClientDto dto, string userId, string userName, CancellationToken cancellationToken = default);
    Task<ClientDto> UpdateAsync(UpdateClientDto dto, string userId, string userName, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string userId, string userName, CancellationToken cancellationToken = default);
}
