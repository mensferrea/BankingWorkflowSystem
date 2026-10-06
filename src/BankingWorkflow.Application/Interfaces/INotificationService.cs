using BankingWorkflow.Application.DTOs.Notifications;
using BankingWorkflow.Domain.Enums;

namespace BankingWorkflow.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsForUserAsync(
        string? userId,
        UserRole? userRole,
        int count = 10,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        string? userId,
        UserRole? userRole,
        CancellationToken cancellationToken = default);

    Task CreateNotificationAsync(
        string? userId,
        UserRole? targetRole,
        string title,
        string message,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string? userId, UserRole? userRole, CancellationToken cancellationToken = default);
}
