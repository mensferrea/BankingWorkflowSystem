using BankingWorkflow.Application.DTOs.Auth;

namespace BankingWorkflow.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
}
