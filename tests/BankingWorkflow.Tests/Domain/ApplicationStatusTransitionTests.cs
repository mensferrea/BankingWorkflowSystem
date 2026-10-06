using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Domain.Exceptions;
using Xunit;
using AppEntity = BankingWorkflow.Domain.Entities.Application;

namespace BankingWorkflow.Tests.Domain;

public class ApplicationStatusTransitionTests
{
    [Fact]
    public void NewApplication_CanTransitionTo_InProgress()
    {
        var app = new AppEntity { Status = ApplicationStatus.New };

        var can = app.CanTransitionTo(ApplicationStatus.InProgress);

        Assert.True(can);
    }

    [Fact]
    public void NewApplication_CannotTransitionDirectlyTo_ApprovedOrRejected()
    {
        var app = new AppEntity { Status = ApplicationStatus.New };

        Assert.False(app.CanTransitionTo(ApplicationStatus.Approved));
        Assert.False(app.CanTransitionTo(ApplicationStatus.Rejected));
    }

    [Fact]
    public void InProgressApplication_CanTransitionTo_ApprovedOrRejected()
    {
        var app = new AppEntity { Status = ApplicationStatus.InProgress };

        Assert.True(app.CanTransitionTo(ApplicationStatus.Approved));
        Assert.True(app.CanTransitionTo(ApplicationStatus.Rejected));
    }

    [Fact]
    public void ApprovedApplication_IsTerminal_CannotTransition()
    {
        var app = new AppEntity { Status = ApplicationStatus.Approved };

        Assert.False(app.CanTransitionTo(ApplicationStatus.New));
        Assert.False(app.CanTransitionTo(ApplicationStatus.InProgress));
        Assert.False(app.CanTransitionTo(ApplicationStatus.Rejected));
    }

    [Fact]
    public void ChangeStatus_FromNewToApproved_ThrowsDomainException()
    {
        var app = new AppEntity { Status = ApplicationStatus.New };

        Action act = () => app.ChangeStatus(ApplicationStatus.Approved, "Решение", "user-1", "Менеджер");
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void ChangeStatus_ApprovedWithoutComment_ThrowsDomainException()
    {
        var app = new AppEntity { Status = ApplicationStatus.InProgress };

        Action act = () => app.ChangeStatus(ApplicationStatus.Approved, null, "user-1", "Менеджер");
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void ChangeStatus_ValidTransition_UpdatesStatusAndAddsHistory()
    {
        var app = new AppEntity { Status = ApplicationStatus.New };

        app.ChangeStatus(ApplicationStatus.InProgress, "Взято в работу", "user-1", "Оператор");

        Assert.Equal(ApplicationStatus.InProgress, app.Status);
        Assert.Single(app.StatusHistory);
        var history = app.StatusHistory.First();
        Assert.Equal(ApplicationStatus.New, history.OldStatus);
        Assert.Equal(ApplicationStatus.InProgress, history.NewStatus);
        Assert.Equal("Взято в работу", history.Comment);
    }
}
