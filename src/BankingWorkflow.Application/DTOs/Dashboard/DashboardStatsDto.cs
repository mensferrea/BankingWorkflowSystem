using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Domain.Enums;

namespace BankingWorkflow.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalApplications { get; set; }
    public int NewApplications { get; set; }
    public int InProgressApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public decimal TotalApprovedAmount { get; set; }
    public decimal TotalRequestedAmount { get; set; }
    public double ApprovalRatePercentage => TotalApplications == 0
        ? 0
        : Math.Round((double)ApprovedApplications / TotalApplications * 100, 1);
    public int TotalClients { get; set; }
    public List<ApplicationSummaryDto> RecentApplications { get; set; } = new();
    public Dictionary<ProductType, int> ApplicationsByProduct { get; set; } = new();
    public Dictionary<ApplicationStatus, int> ApplicationsByStatus { get; set; } = new();
}
