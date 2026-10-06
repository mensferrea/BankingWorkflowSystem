using BankingWorkflow.Domain.Enums;

namespace BankingWorkflow.Application.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public UserRole? TargetRole { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? ApplicationId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
