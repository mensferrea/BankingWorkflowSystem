using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Domain.Exceptions;

namespace BankingWorkflow.Domain.Entities;

public class Application
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ApplicationNumber { get; set; } = string.Empty;

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public ProductType ProductType { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
    public string? StatusComment { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    public ICollection<AttachedDocument> Documents { get; set; } = new List<AttachedDocument>();
    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();

    public decimal MonthlyPayment
    {
        get
        {
            if (LoanTermMonths <= 0 || LoanAmount <= 0) return 0m;
            if (InterestRate <= 0) return Math.Round(LoanAmount / LoanTermMonths, 2);

            var monthlyRate = (double)(InterestRate / 100m / 12m);
            var factor = Math.Pow(1.0 + monthlyRate, LoanTermMonths);
            var payment = (double)LoanAmount * (monthlyRate * factor) / (factor - 1.0);
            return Math.Round((decimal)payment, 2);
        }
    }

    public decimal TotalPayment => Math.Round(MonthlyPayment * LoanTermMonths, 2);
    public decimal TotalOverpayment => Math.Max(0m, TotalPayment - LoanAmount);

    public bool CanTransitionTo(ApplicationStatus newStatus)
    {
        return (Status, newStatus) switch
        {
            (ApplicationStatus.New, ApplicationStatus.InProgress) => true,
            (ApplicationStatus.InProgress, ApplicationStatus.Approved) => true,
            (ApplicationStatus.InProgress, ApplicationStatus.Rejected) => true,
            _ => false
        };
    }

    public void ChangeStatus(
        ApplicationStatus newStatus,
        string? comment,
        string userId,
        string userName)
    {
        if (Status == newStatus) return;

        if (!CanTransitionTo(newStatus))
        {
            throw new DomainException(
                $"Недопустимый переход статуса из '{GetStatusTitle(Status)}' в '{GetStatusTitle(newStatus)}'. " +
                "Допустимый жизненный цикл: NEW → IN_PROGRESS → APPROVED / REJECTED.");
        }

        if ((newStatus == ApplicationStatus.Approved || newStatus == ApplicationStatus.Rejected) &&
            string.IsNullOrWhiteSpace(comment))
        {
            throw new DomainException("Для утверждения или отклонения заявки требуется указать комментарий/обоснование решения.");
        }

        var oldStatus = Status;
        Status = newStatus;
        StatusComment = comment?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;

        StatusHistory.Add(new ApplicationStatusHistory
        {
            ApplicationId = Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Comment = comment?.Trim(),
            ChangedByUserId = userId,
            ChangedByUserName = userName,
            ChangedAt = DateTimeOffset.UtcNow
        });
    }

    public static string GetStatusTitle(ApplicationStatus status) => status switch
    {
        ApplicationStatus.New => "Новая",
        ApplicationStatus.InProgress => "В обработке",
        ApplicationStatus.Approved => "Одобрена",
        ApplicationStatus.Rejected => "Отклонена",
        _ => status.ToString()
    };
}
