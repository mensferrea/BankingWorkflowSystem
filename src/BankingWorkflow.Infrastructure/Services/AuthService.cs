using BankingWorkflow.Application.DTOs.Auth;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BankingWorkflow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditService _auditService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IAuditService auditService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _auditService = auditService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponseDto { Success = false, ErrorMessage = "Неверный email или пароль." };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return new AuthResponseDto { Success = false, ErrorMessage = "Неверный email или пароль." };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? user.Role.ToString();

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, role);

        await _auditService.LogAsync(
            user.Id.ToString(),
            user.FullName,
            "LOGIN",
            "User",
            user.Id.ToString(),
            $"Пользователь {user.Email} успешно вошел в систему под ролью {role}",
            cancellationToken: cancellationToken);

        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = role
            }
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
        {
            return new AuthResponseDto { Success = false, ErrorMessage = "Пользователь с таким email уже существует." };
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role,
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description))
            };
        }

        var roleName = request.Role.ToString();
        await _userManager.AddToRoleAsync(user, roleName);

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, roleName);

        await _auditService.LogAsync(
            user.Id.ToString(),
            user.FullName,
            "REGISTER_USER",
            "User",
            user.Id.ToString(),
            $"Зарегистрирован новый пользователь {user.Email} с ролью {roleName}",
            cancellationToken: cancellationToken);

        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = roleName
            }
        };
    }

    public async Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? user.Role.ToString()
            });
        }

        return result;
    }
}
