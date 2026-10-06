using BankingWorkflow.Domain.Enums;

namespace BankingWorkflow.Application.DTOs.Clients;

public class ClientDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string PassportSeries { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public string RegistrationAddress { get; set; } = string.Empty;
    public CreditRating CreditRating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int ActiveApplicationsCount { get; set; }
}

public class CreateClientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string PassportSeries { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Employed;
    public string RegistrationAddress { get; set; } = string.Empty;
    public CreditRating CreditRating { get; set; } = CreditRating.Good;
}

public class UpdateClientDto : CreateClientDto
{
    public Guid Id { get; set; }
}

public class ClientLookupDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Passport { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
}
