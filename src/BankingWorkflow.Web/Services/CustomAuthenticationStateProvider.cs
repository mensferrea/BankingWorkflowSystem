using System.Security.Claims;
using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Infrastructure.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BankingWorkflow.Web.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IServiceProvider _serviceProvider;
    private ClaimsPrincipal _currentPrincipal = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentPrincipal));
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return false;

        var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) return false;

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? user.Role.ToString();

        SetUser(user, role);
        return true;
    }

    public async Task SwitchDemoRoleAsync(UserRole role)
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var email = role switch
        {
            UserRole.Admin => "admin@bank.local",
            UserRole.Manager => "manager@bank.local",
            _ => "operator@bank.local"
        };

        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var roles = await userManager.GetRolesAsync(user);
            SetUser(user, roles.FirstOrDefault() ?? role.ToString());
        }
    }

    public void Logout()
    {
        _currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentPrincipal)));
    }

    private void SetUser(ApplicationUser user, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "CustomAuth");
        _currentPrincipal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentPrincipal)));
    }
}
