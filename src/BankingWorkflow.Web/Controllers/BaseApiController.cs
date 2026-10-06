using System.Security.Claims;
using BankingWorkflow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BankingWorkflow.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        string.Empty;

    protected string CurrentUserName =>
        User.FindFirstValue(ClaimTypes.Name) ??
        User.FindFirstValue(ClaimTypes.Email) ??
        "Пользователь API";

    protected UserRole CurrentUserRole
    {
        get
        {
            var roleStr = User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(roleStr, true, out var role)
                ? role
                : UserRole.Operator;
        }
    }
}
