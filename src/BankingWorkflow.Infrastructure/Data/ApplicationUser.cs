using BankingWorkflow.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BankingWorkflow.Infrastructure.Data;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Operator;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
