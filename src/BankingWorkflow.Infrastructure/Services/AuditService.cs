using BankingWorkflow.Application.DTOs.Audit;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingWorkflow.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly BankingDbContext _db;

    public AuditService(BankingDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        string? userId,
        string userName,
        string action,
        string entityName,
        string? entityId,
        string details,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserName = string.IsNullOrWhiteSpace(userName) ? "Система" : userName,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTimeOffset.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<AuditLogDto> Items, int TotalCount)> GetPagedAsync(
        AuditLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(x =>
                x.UserName.ToLower().Contains(search) ||
                x.Action.ToLower().Contains(search) ||
                x.Details.ToLower().Contains(search) ||
                x.EntityName.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(x => x.Action == filter.Action);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            query = query.Where(x => x.EntityName == filter.EntityName);
        }

        if (filter.From.HasValue)
        {
            query = query.Where(x => x.Timestamp >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(x => x.Timestamp <= filter.To.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.UserName,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Details = x.Details,
                IpAddress = x.IpAddress,
                Timestamp = x.Timestamp
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
