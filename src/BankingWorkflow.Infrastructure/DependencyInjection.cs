using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Application.DTOs.Clients;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Application.Validators;
using BankingWorkflow.Infrastructure.Data;
using BankingWorkflow.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingWorkflow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBankingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=banking_workflow;Username=postgres;Password=postgres";

        services.AddDbContext<BankingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(BankingDbContext).Assembly.FullName);
            });
        });

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<BankingDbContext>()
        .AddSignInManager<SignInManager<ApplicationUser>>()
        .AddDefaultTokenProviders();

        services.AddScoped<IValidator<CreateClientDto>, CreateClientValidator>();
        services.AddScoped<IValidator<CreateApplicationDto>, CreateApplicationValidator>();
        services.AddScoped<IValidator<ChangeStatusDto>, ChangeStatusValidator>();

        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        services.AddScoped<IDocumentGenerationService, WordDocumentGenerationService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
