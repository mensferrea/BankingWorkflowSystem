using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Domain.Exceptions;

namespace BankingWorkflow.Domain.Entities;

public class Client
{
    public Guid Id { get; set; } = Guid.NewGuid();
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
    public EmploymentStatus EmploymentStatus { get; set; }
    public string RegistrationAddress { get; set; } = string.Empty;
    public CreditRating CreditRating { get; set; } = CreditRating.Good;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Application> Applications { get; set; } = new List<Application>();

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}"
        : $"{LastName} {FirstName} {MiddleName}";

    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - BirthDate.Year;
            if (BirthDate > today.AddYears(-age)) age--;
            return age;
        }
    }

    public void UpdateDetails(
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        string passportSeries,
        string passportNumber,
        string taxNumber,
        string phoneNumber,
        string email,
        decimal monthlyIncome,
        EmploymentStatus employmentStatus,
        string registrationAddress,
        CreditRating creditRating)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("Имя клиента обязательно для заполнения.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Фамилия клиента обязательна для заполнения.");
        if (monthlyIncome < 0)
            throw new DomainException("Ежемесячный доход не может быть отрицательным.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim();
        BirthDate = birthDate;
        PassportSeries = passportSeries.Trim();
        PassportNumber = passportNumber.Trim();
        TaxNumber = taxNumber.Trim();
        PhoneNumber = phoneNumber.Trim();
        Email = email.Trim();
        MonthlyIncome = monthlyIncome;
        EmploymentStatus = employmentStatus;
        RegistrationAddress = registrationAddress.Trim();
        CreditRating = creditRating;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
