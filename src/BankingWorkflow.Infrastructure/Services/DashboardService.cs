using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Application.DTOs.Dashboard;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AppEntity = BankingWorkflow.Domain.Entities.Application;

namespace BankingWorkflow.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly BankingDbContext _db;

    public DashboardService(BankingDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var applications = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Client)
            .Include(a => a.Documents)
            .ToListAsync(cancellationToken);

        var totalClients = await _db.Clients.CountAsync(cancellationToken);

        var stats = new DashboardStatsDto
        {
            TotalApplications = applications.Count,
            NewApplications = applications.Count(a => a.Status == ApplicationStatus.New),
            InProgressApplications = applications.Count(a => a.Status == ApplicationStatus.InProgress),
            ApprovedApplications = applications.Count(a => a.Status == ApplicationStatus.Approved),
            RejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Rejected),
            TotalApprovedAmount = applications.Where(a => a.Status == ApplicationStatus.Approved).Sum(a => a.LoanAmount),
            TotalRequestedAmount = applications.Sum(a => a.LoanAmount),
            TotalClients = totalClients,
            ApplicationsByStatus = Enum.GetValues<ApplicationStatus>()
                .ToDictionary(s => s, s => applications.Count(a => a.Status == s)),
            ApplicationsByProduct = Enum.GetValues<ProductType>()
                .ToDictionary(p => p, p => applications.Count(a => a.ProductType == p)),
            RecentApplications = applications
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new ApplicationSummaryDto
                {
                    Id = a.Id,
                    ApplicationNumber = a.ApplicationNumber,
                    ClientId = a.ClientId,
                    ClientFullName = a.Client?.FullName ?? string.Empty,
                    ClientTaxNumber = a.Client?.TaxNumber ?? string.Empty,
                    LoanAmount = a.LoanAmount,
                    LoanTermMonths = a.LoanTermMonths,
                    InterestRate = a.InterestRate,
                    ProductType = a.ProductType,
                    Status = a.Status,
                    StatusTitle = AppEntity.GetStatusTitle(a.Status),
                    CreatedAt = a.CreatedAt,
                    DocumentsCount = a.Documents.Count
                })
                .ToList()
        };

        return stats;
    }
}
