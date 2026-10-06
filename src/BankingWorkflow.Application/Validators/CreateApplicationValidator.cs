using BankingWorkflow.Application.DTOs.Applications;
using FluentValidation;

namespace BankingWorkflow.Application.Validators;

public class CreateApplicationValidator : AbstractValidator<CreateApplicationDto>
{
    public CreateApplicationValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Клиент должен быть выбран");

        RuleFor(x => x.LoanAmount)
            .GreaterThanOrEqualTo(10_000).WithMessage("Минимальная сумма заявки — 10 000 ₽")
            .LessThanOrEqualTo(100_000_000).WithMessage("Максимальная сумма заявки — 100 000 000 ₽");

        RuleFor(x => x.LoanTermMonths)
            .InclusiveBetween(3, 360).WithMessage("Срок кредитования должен быть от 3 до 360 месяцев (до 30 лет)");

        RuleFor(x => x.InterestRate)
            .InclusiveBetween(1.0m, 99.0m).WithMessage("Процентная ставка должна быть от 1% до 99%");

        RuleFor(x => x.ProductType)
            .IsInEnum().WithMessage("Некорректный тип кредитного продукта");
    }
}
