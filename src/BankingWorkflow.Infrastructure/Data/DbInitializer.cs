using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AppEntity = BankingWorkflow.Domain.Entities.Application;

namespace BankingWorkflow.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await db.Database.MigrateAsync();

        var roles = new[] { UserRole.Admin.ToString(), UserRole.Manager.ToString(), UserRole.Operator.ToString() };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var adminUser = await EnsureUserAsync(
            userManager,
            "admin@bank.local",
            "Администратор Системы",
            "Admin123!",
            UserRole.Admin);

        var managerUser = await EnsureUserAsync(
            userManager,
            "manager@bank.local",
            "Иванов Петр Сергеевич",
            "Manager123!",
            UserRole.Manager);

        var operatorUser = await EnsureUserAsync(
            userManager,
            "operator@bank.local",
            "Смирнова Анна Дмитриевна",
            "Operator123!",
            UserRole.Operator);

        if (!await db.Clients.AnyAsync())
        {
            var client1 = new Client
            {
                FirstName = "Алексей",
                LastName = "Кузнецов",
                MiddleName = "Владимирович",
                BirthDate = new DateOnly(1988, 5, 14),
                PassportSeries = "4512",
                PassportNumber = "892341",
                TaxNumber = "772345678901",
                PhoneNumber = "+7 (916) 123-45-67",
                Email = "kuznetsov.alex@example.com",
                MonthlyIncome = 185_000,
                EmploymentStatus = EmploymentStatus.Employed,
                RegistrationAddress = "г. Москва, ул. Тверская, д. 12, кв. 45",
                CreditRating = CreditRating.Excellent,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-30),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-30)
            };

            var client2 = new Client
            {
                FirstName = "Екатерина",
                LastName = "Морозова",
                MiddleName = "Сергеевна",
                BirthDate = new DateOnly(1995, 9, 21),
                PassportSeries = "4015",
                PassportNumber = "567123",
                TaxNumber = "781298765432",
                PhoneNumber = "+7 (921) 345-67-89",
                Email = "morozova.katya@example.com",
                MonthlyIncome = 95_000,
                EmploymentStatus = EmploymentStatus.Employed,
                RegistrationAddress = "г. Санкт-Петербург, Невский пр-т, д. 88, кв. 14",
                CreditRating = CreditRating.Good,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-20),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-20)
            };

            var client3 = new Client
            {
                FirstName = "Дмитрий",
                LastName = "Васильев",
                MiddleName = "Николаевич",
                BirthDate = new DateOnly(1982, 11, 3),
                PassportSeries = "2208",
                PassportNumber = "431890",
                TaxNumber = "526012345678",
                PhoneNumber = "+7 (903) 789-01-23",
                Email = "vasiliev.dmitry@example.com",
                MonthlyIncome = 240_000,
                EmploymentStatus = EmploymentStatus.BusinessOwner,
                RegistrationAddress = "г. Нижний Новгород, ул. Белинского, д. 5, кв. 22",
                CreditRating = CreditRating.Excellent,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-15),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-15)
            };

            var client4 = new Client
            {
                FirstName = "Ольга",
                LastName = "Попова",
                MiddleName = "Игоревна",
                BirthDate = new DateOnly(1992, 3, 10),
                PassportSeries = "6011",
                PassportNumber = "654321",
                TaxNumber = "616409876543",
                PhoneNumber = "+7 (988) 456-78-90",
                Email = "popova.olga@example.com",
                MonthlyIncome = 45_000,
                EmploymentStatus = EmploymentStatus.SelfEmployed,
                RegistrationAddress = "г. Ростов-на-Дону, ул. Пушкинская, д. 101, кв. 19",
                CreditRating = CreditRating.Fair,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            };

            var client5 = new Client
            {
                FirstName = "Роман",
                LastName = "Соколов",
                MiddleName = "Андреевич",
                BirthDate = new DateOnly(2001, 8, 17),
                PassportSeries = "3619",
                PassportNumber = "123789",
                TaxNumber = "631567890123",
                PhoneNumber = "+7 (999) 111-22-33",
                Email = "sokolov.roman@example.com",
                MonthlyIncome = 28_000,
                EmploymentStatus = EmploymentStatus.Unemployed,
                RegistrationAddress = "г. Самара, Московское шоссе, д. 43, кв. 7",
                CreditRating = CreditRating.Poor,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            };

            db.Clients.AddRange(client1, client2, client3, client4, client5);
            await db.SaveChangesAsync();

            var app1 = new AppEntity
            {
                ApplicationNumber = "APP-20261001-0001",
                ClientId = client1.Id,
                LoanAmount = 4_500_000,
                LoanTermMonths = 120,
                InterestRate = 12.5m,
                ProductType = ProductType.Mortgage,
                Status = ApplicationStatus.Approved,
                StatusComment = "Пакет документов полный, кредитная история безупречна, платежеспособность подтверждена.",
                CreatedByUserId = operatorUser.Id.ToString(),
                CreatedByUserName = operatorUser.FullName,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-28),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-25)
            };
            app1.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app1.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.New,
                Comment = "Заявка зарегистрирована в офисе",
                ChangedByUserId = operatorUser.Id.ToString(),
                ChangedByUserName = operatorUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-28)
            });
            app1.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app1.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.InProgress,
                Comment = "Передано на андеррайтинг",
                ChangedByUserId = operatorUser.Id.ToString(),
                ChangedByUserName = operatorUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-27)
            });
            app1.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app1.Id,
                OldStatus = ApplicationStatus.InProgress,
                NewStatus = ApplicationStatus.Approved,
                Comment = "Заявка одобрена кредитным комитетом",
                ChangedByUserId = managerUser.Id.ToString(),
                ChangedByUserName = managerUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-25)
            });

            var app2 = new AppEntity
            {
                ApplicationNumber = "APP-20261002-0002",
                ClientId = client2.Id,
                LoanAmount = 850_000,
                LoanTermMonths = 36,
                InterestRate = 15.9m,
                ProductType = ProductType.ConsumerLoan,
                Status = ApplicationStatus.InProgress,
                StatusComment = "Запрос справки 2-НДФЛ у работодателя",
                CreatedByUserId = operatorUser.Id.ToString(),
                CreatedByUserName = operatorUser.FullName,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-12),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            };
            app2.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app2.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.New,
                Comment = "Заявка принята через онлайн-кабинет",
                ChangedByUserId = operatorUser.Id.ToString(),
                ChangedByUserName = operatorUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-12)
            });
            app2.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app2.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.InProgress,
                Comment = "Проверка данных скоринга",
                ChangedByUserId = managerUser.Id.ToString(),
                ChangedByUserName = managerUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-10)
            });

            var app3 = new AppEntity
            {
                ApplicationNumber = "APP-20261003-0003",
                ClientId = client3.Id,
                LoanAmount = 7_000_000,
                LoanTermMonths = 60,
                InterestRate = 14.0m,
                ProductType = ProductType.BusinessLoan,
                Status = ApplicationStatus.Approved,
                StatusComment = "Финансовая отчетность предприятия устойчивая, залог оборудования оформлен.",
                CreatedByUserId = operatorUser.Id.ToString(),
                CreatedByUserName = operatorUser.FullName,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-8),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-6)
            };
            app3.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app3.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.New,
                Comment = "Первичная регистрация пакета бизнеса",
                ChangedByUserId = operatorUser.Id.ToString(),
                ChangedByUserName = operatorUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-8)
            });
            app3.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app3.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.InProgress,
                Comment = "Оценка залогового имущества",
                ChangedByUserId = operatorUser.Id.ToString(),
                ChangedByUserName = operatorUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-7)
            });
            app3.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app3.Id,
                OldStatus = ApplicationStatus.InProgress,
                NewStatus = ApplicationStatus.Approved,
                Comment = "Кредит согласован с комитетом рисков",
                ChangedByUserId = managerUser.Id.ToString(),
                ChangedByUserName = managerUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-6)
            });

            var app4 = new AppEntity
            {
                ApplicationNumber = "APP-20261004-0004",
                ClientId = client4.Id,
                LoanAmount = 350_000,
                LoanTermMonths = 24,
                InterestRate = 18.5m,
                ProductType = ProductType.ConsumerLoan,
                Status = ApplicationStatus.New,
                CreatedByUserId = operatorUser.Id.ToString(),
                CreatedByUserName = operatorUser.FullName,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            };
            app4.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app4.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.New,
                Comment = "Подана новая заявка",
                ChangedByUserId = operatorUser.Id.ToString(),
                ChangedByUserName = operatorUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-2)
            });

            var app5 = new AppEntity
            {
                ApplicationNumber = "APP-20261005-0005",
                ClientId = client5.Id,
                LoanAmount = 1_200_000,
                LoanTermMonths = 48,
                InterestRate = 16.0m,
                ProductType = ProductType.AutoLoan,
                Status = ApplicationStatus.Rejected,
                StatusComment = "Высокая долговая нагрузка, отсутствие подтвержденного официального дохода, низкий скоринг.",
                CreatedByUserId = operatorUser.Id.ToString(),
                CreatedByUserName = operatorUser.FullName,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-4),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-3)
            };
            app5.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app5.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.New,
                Comment = "Заявка принята в работу",
                ChangedByUserId = operatorUser.Id.ToString(),
                ChangedByUserName = operatorUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-4)
            });
            app5.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app5.Id,
                OldStatus = ApplicationStatus.New,
                NewStatus = ApplicationStatus.InProgress,
                Comment = "Андеррайтинг рисков",
                ChangedByUserId = operatorUser.Id.ToString(),
                ChangedByUserName = operatorUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-4)
            });
            app5.StatusHistory.Add(new ApplicationStatusHistory
            {
                ApplicationId = app5.Id,
                OldStatus = ApplicationStatus.InProgress,
                NewStatus = ApplicationStatus.Rejected,
                Comment = "Отказ службы безопасности и кредитных рисков",
                ChangedByUserId = managerUser.Id.ToString(),
                ChangedByUserName = managerUser.FullName,
                ChangedAt = DateTimeOffset.UtcNow.AddDays(-3)
            });

            db.Applications.AddRange(app1, app2, app3, app4, app5);

            db.Notifications.AddRange(
                new Notification
                {
                    UserId = operatorUser.Id.ToString(),
                    TargetRole = UserRole.Operator,
                    Title = "Заявка APP-20261001-0001 одобрена",
                    Message = "Менеджер Иванов П.С. утвердил заявку на ипотечный кредит на сумму 4 500 000 ₽",
                    ApplicationId = app1.Id,
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-25)
                },
                new Notification
                {
                    UserId = null,
                    TargetRole = UserRole.Manager,
                    Title = "Новая заявка APP-20261004-0004 ожидает рассмотрения",
                    Message = "Поступила заявка на потребительский кредит от клиента Попова О.И. (350 000 ₽)",
                    ApplicationId = app4.Id,
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
                }
            );

            db.AuditLogs.AddRange(
                new AuditLog
                {
                    UserId = adminUser.Id.ToString(),
                    UserName = adminUser.FullName,
                    Action = "SYSTEM_INITIALIZED",
                    EntityName = "System",
                    Details = "Система банковского документооборота успешно инициализирована",
                    Timestamp = DateTimeOffset.UtcNow.AddDays(-30)
                },
                new AuditLog
                {
                    UserId = operatorUser.Id.ToString(),
                    UserName = operatorUser.FullName,
                    Action = "CREATE_APPLICATION",
                    EntityName = "Application",
                    EntityId = app1.Id.ToString(),
                    Details = "Создана заявка APP-20261001-0001",
                    Timestamp = DateTimeOffset.UtcNow.AddDays(-28)
                },
                new AuditLog
                {
                    UserId = managerUser.Id.ToString(),
                    UserName = managerUser.FullName,
                    Action = "CHANGE_STATUS",
                    EntityName = "Application",
                    EntityId = app1.Id.ToString(),
                    Details = "Заявка APP-20261001-0001 переведена в статус 'Одобрена'",
                    Timestamp = DateTimeOffset.UtcNow.AddDays(-25)
                }
            );

            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        UserRole role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Role = role,
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role.ToString());
        }

        return user;
    }
}
