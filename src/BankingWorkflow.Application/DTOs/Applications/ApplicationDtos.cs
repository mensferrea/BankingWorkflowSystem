using BankingWorkflow.Application.DTOs.Clients;
using BankingWorkflow.Application.DTOs.Documents;
using BankingWorkflow.Domain.Enums;

namespace BankingWorkflow.Application.DTOs.Applications;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public ClientDto? Client { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public ProductType ProductType { get; set; }
    public ApplicationStatus Status { get; set; }
    public string StatusTitle { get; set; } = string.Empty;
    public string? StatusComment { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal TotalOverpayment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public List<DocumentDto> Documents { get; set; } = new();
    public List<StatusHistoryDto> StatusHistory { get; set; } = new();
}

public class CreateApplicationDto
{
    public Guid ClientId { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public ProductType ProductType { get; set; } = ProductType.ConsumerLoan;
}

public class UpdateApplicationDto
{
    public Guid Id { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public ProductType ProductType { get; set; }
}

public class ChangeStatusDto
{
    public ApplicationStatus NewStatus { get; set; }
    public string? Comment { get; set; }
}

public class ApplicationFilterDto
{
    public string? Search { get; set; }
    public ApplicationStatus? Status { get; set; }
    public ProductType? ProductType { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ApplicationSummaryDto
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientFullName { get; set; } = string.Empty;
    public string ClientTaxNumber { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public ProductType ProductType { get; set; }
    public ApplicationStatus Status { get; set; }
    public string StatusTitle { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public int DocumentsCount { get; set; }
}

public class StatusHistoryDto
{
    public Guid Id { get; set; }
    public ApplicationStatus OldStatus { get; set; }
    public string OldStatusTitle { get; set; } = string.Empty;
    public ApplicationStatus NewStatus { get; set; }
    public string NewStatusTitle { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }
}
