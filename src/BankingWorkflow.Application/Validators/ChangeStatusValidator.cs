using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Domain.Enums;
using FluentValidation;

namespace BankingWorkflow.Application.Validators;

public class ChangeStatusValidator : AbstractValidator<ChangeStatusDto>
{
    public ChangeStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Некорректный целевой статус");

        When(x => x.NewStatus == ApplicationStatus.Approved || x.NewStatus == ApplicationStatus.Rejected, () =>
        {
            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("При принятии решения (одобрение/отклонение) комментарий обязателен")
                .MinimumLength(3).WithMessage("Комментарий должен содержать не менее 3 символов");
        });
    }
}
