using System.Text.RegularExpressions;
using BankingWorkflow.Application.DTOs.Clients;
using FluentValidation;

namespace BankingWorkflow.Application.Validators;

public class CreateClientValidator : AbstractValidator<CreateClientDto>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя клиента обязательно")
            .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия клиента обязательна")
            .MaximumLength(100).WithMessage("Фамилия не должна превышать 100 символов");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Дата рождения обязательна")
            .Must(BeAtLeast18YearsOld).WithMessage("Клиент должен быть совершеннолетним (не менее 18 лет)");

        RuleFor(x => x.PassportSeries)
            .NotEmpty().WithMessage("Серия паспорта обязательна")
            .Matches(@"^\d{4}$").WithMessage("Серия паспорта должна состоять из 4 цифр");

        RuleFor(x => x.PassportNumber)
            .NotEmpty().WithMessage("Номер паспорта обязателен")
            .Matches(@"^\d{6}$").WithMessage("Номер паспорта должен состоять из 6 цифр");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("ИНН обязателен")
            .Matches(@"^\d{10}(\d{2})?$").WithMessage("ИНН должен содержать 10 или 12 цифр");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона обязателен")
            .Matches(@"^(\+7|8)?[\s\-]?\(?[0-9]{3}\)?[\s\-]?[0-9]{3}[\s\-]?[0-9]{2}[\s\-]?[0-9]{2}$")
            .WithMessage("Некорректный формат телефонного номера");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат Email адреса");

        RuleFor(x => x.MonthlyIncome)
            .GreaterThan(0).WithMessage("Ежемесячный доход должен быть больше 0");

        RuleFor(x => x.RegistrationAddress)
            .NotEmpty().WithMessage("Адрес регистрации обязателен")
            .MinimumLength(5).WithMessage("Адрес регистрации слишком короткий");
    }

    private static bool BeAtLeast18YearsOld(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age >= 18;
    }
}
