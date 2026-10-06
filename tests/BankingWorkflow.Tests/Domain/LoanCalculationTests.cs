using BankingWorkflow.Domain.Entities;
using Xunit;
using AppEntity = BankingWorkflow.Domain.Entities.Application;

namespace BankingWorkflow.Tests.Domain;

public class LoanCalculationTests
{
    [Fact]
    public void MonthlyPayment_WithZeroInterest_DividesEqually()
    {
        var app = new AppEntity
        {
            LoanAmount = 120_000,
            LoanTermMonths = 12,
            InterestRate = 0
        };

        Assert.Equal(10_000m, app.MonthlyPayment);
        Assert.Equal(120_000m, app.TotalPayment);
        Assert.Equal(0m, app.TotalOverpayment);
    }

    [Fact]
    public void MonthlyPayment_WithAnnuityInterest_CalculatesCorrectly()
    {
        var app = new AppEntity
        {
            LoanAmount = 1_000_000,
            LoanTermMonths = 12,
            InterestRate = 12.0m
        };

        Assert.True(app.MonthlyPayment > 83_333m);
        Assert.True(app.TotalPayment > app.LoanAmount);
        Assert.True(app.TotalOverpayment > 0);
    }
}
