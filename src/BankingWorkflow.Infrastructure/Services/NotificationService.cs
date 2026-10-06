using BankingWorkflow.Application.DTOs.Notifications;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingWorkflow.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly BankingDbContext _db;

    public NotificationService(BankingDbContext db)
    {
        _db = db;
    }

    public async Task<List<NotificationDto>> GetNotificationsForUserAsync(
        string? userId,
        UserRole? userRole,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications.AsNoTracking();

        if (userRole == UserRole.Admin)
        {
            // Admin sees all notifications
        }
        else if (userRole.HasValue)
        {
            query = query.Where(n => n.UserId == userId || n.TargetRole == userRole || n.TargetRole == null);
        }
        else if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(n => n.UserId == userId || n.TargetRole == null);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                TargetRole = n.TargetRole,
                Title = n.Title,
                Message = n.Message,
                ApplicationId = n.ApplicationId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        string? userId,
        UserRole? userRole,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications.Where(n => !n.IsRead);

        if (userRole == UserRole.Admin)
        {
            return await query.CountAsync(cancellationToken);
        }

        if (userRole.HasValue)
        {
            query = query.Where(n => n.UserId == userId || n.TargetRole == userRole || n.TargetRole == null);
        }
        else if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(n => n.UserId == userId || n.TargetRole == null);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task CreateNotificationAsync(
        string? userId,
        UserRole? targetRole,
        string title,
        string message,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            TargetRole = targetRole,
            Title = title,
            Message = message,
            ApplicationId = applicationId,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _db.Notifications.FindAsync(new object[] { id }, cancellationToken);
        if (notification != null)
        {
            notification.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(string? userId, UserRole? userRole, CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications.Where(n => !n.IsRead);

        if (userRole != UserRole.Admin)
        {
            if (userRole.HasValue)
            {
                query = query.Where(n => n.UserId == userId || n.TargetRole == userRole || n.TargetRole == null);
            }
            else if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(n => n.UserId == userId);
            }
        }

        var unread = await query.ToListAsync(cancellationToken);
        foreach (var item in unread)
        {
            item.IsRead = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
