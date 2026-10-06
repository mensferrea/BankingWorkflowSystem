using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Application.DTOs.Clients;
using BankingWorkflow.Application.Validators;
using BankingWorkflow.Domain.Enums;
using Xunit;

namespace BankingWorkflow.Tests.Application;

public class ValidatorTests
{
    private readonly CreateClientValidator _clientValidator = new();
    private readonly CreateApplicationValidator _applicationValidator = new();

    [Fact]
    public void ValidClient_PassesValidation()
    {
        var dto = new CreateClientDto
        {
            FirstName = "Иван",
            LastName = "Иванов",
            BirthDate = new DateOnly(1990, 1, 1),
            PassportSeries = "4510",
            PassportNumber = "123456",
            TaxNumber = "771234567890",
            PhoneNumber = "+7 (999) 123-45-67",
            Email = "ivanov@example.com",
            MonthlyIncome = 100_000,
            RegistrationAddress = "г. Москва, ул. Ленина, д. 1"
        };

        var result = _clientValidator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UnderageClient_FailsValidation()
    {
        var dto = new CreateClientDto
        {
            FirstName = "Петр",
            LastName = "Сидоров",
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-17)),
            PassportSeries = "4510",
            PassportNumber = "123456",
            TaxNumber = "771234567890",
            PhoneNumber = "+7 (999) 123-45-67",
            Email = "petr@example.com",
            MonthlyIncome = 50_000,
            RegistrationAddress = "г. Москва"
        };

        var result = _clientValidator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateClientDto.BirthDate));
    }

    [Fact]
    public void InvalidPassport_FailsValidation()
    {
        var dto = new CreateClientDto
        {
            FirstName = "Иван",
            LastName = "Иванов",
            BirthDate = new DateOnly(1990, 1, 1),
            PassportSeries = "12",
            PassportNumber = "123",
            TaxNumber = "771234567890",
            PhoneNumber = "+7 (999) 123-45-67",
            Email = "ivanov@example.com",
            MonthlyIncome = 100_000,
            RegistrationAddress = "г. Москва"
        };

        var result = _clientValidator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateClientDto.PassportSeries));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateClientDto.PassportNumber));
    }

    [Fact]
    public void ValidApplication_PassesValidation()
    {
        var dto = new CreateApplicationDto
        {
            ClientId = Guid.NewGuid(),
            LoanAmount = 500_000,
            LoanTermMonths = 36,
            InterestRate = 15.0m,
            ProductType = ProductType.ConsumerLoan
        };

        var result = _applicationValidator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Application_WithTooLowAmount_FailsValidation()
    {
        var dto = new CreateApplicationDto
        {
            ClientId = Guid.NewGuid(),
            LoanAmount = 500,
            LoanTermMonths = 36,
            InterestRate = 15.0m,
            ProductType = ProductType.ConsumerLoan
        };

        var result = _applicationValidator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateApplicationDto.LoanAmount));
    }
}
