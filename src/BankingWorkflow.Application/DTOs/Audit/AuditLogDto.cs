namespace BankingWorkflow.Application.DTOs.Audit;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Details { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class AuditLogFilterDto
{
    public string? Search { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
}
