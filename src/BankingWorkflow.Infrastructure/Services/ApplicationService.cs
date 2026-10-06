using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Application.DTOs.Clients;
using BankingWorkflow.Application.DTOs.Documents;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Domain.Exceptions;
using BankingWorkflow.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using AppEntity = BankingWorkflow.Domain.Entities.Application;

namespace BankingWorkflow.Infrastructure.Services;

public class ApplicationService : IApplicationService
{
    private readonly BankingDbContext _db;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly IValidator<CreateApplicationDto> _createValidator;
    private readonly IValidator<ChangeStatusDto> _changeStatusValidator;

    public ApplicationService(
        BankingDbContext db,
        IAuditService auditService,
        INotificationService notificationService,
        IValidator<CreateApplicationDto> createValidator,
        IValidator<ChangeStatusDto> changeStatusValidator)
    {
        _db = db;
        _auditService = auditService;
        _notificationService = notificationService;
        _createValidator = createValidator;
        _changeStatusValidator = changeStatusValidator;
    }

    public async Task<(List<ApplicationSummaryDto> Items, int TotalCount)> GetPagedAsync(
        ApplicationFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Applications
            .AsNoTracking()
            .Include(a => a.Client)
            .Include(a => a.Documents)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(a =>
                a.ApplicationNumber.ToLower().Contains(s) ||
                (a.Client != null && (
                    a.Client.LastName.ToLower().Contains(s) ||
                    a.Client.FirstName.ToLower().Contains(s) ||
                    a.Client.TaxNumber.Contains(s) ||
                    a.Client.PhoneNumber.Contains(s))));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(a => a.Status == filter.Status.Value);
        }

        if (filter.ProductType.HasValue)
        {
            query = query.Where(a => a.ProductType == filter.ProductType.Value);
        }

        if (filter.MinAmount.HasValue)
        {
            query = query.Where(a => a.LoanAmount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(a => a.LoanAmount <= filter.MaxAmount.Value);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= filter.DateTo.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "amount" => filter.SortDescending ? query.OrderByDescending(a => a.LoanAmount) : query.OrderBy(a => a.LoanAmount),
            "status" => filter.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "client" => filter.SortDescending
                ? query.OrderByDescending(a => a.Client != null ? a.Client.LastName : string.Empty)
                : query.OrderBy(a => a.Client != null ? a.Client.LastName : string.Empty),
            "number" => filter.SortDescending ? query.OrderByDescending(a => a.ApplicationNumber) : query.OrderBy(a => a.ApplicationNumber),
            _ => filter.SortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt)
        };

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new ApplicationSummaryDto
            {
                Id = a.Id,
                ApplicationNumber = a.ApplicationNumber,
                ClientId = a.ClientId,
                ClientFullName = a.Client != null ? a.Client.FullName : string.Empty,
                ClientTaxNumber = a.Client != null ? a.Client.TaxNumber : string.Empty,
                LoanAmount = a.LoanAmount,
                LoanTermMonths = a.LoanTermMonths,
                InterestRate = a.InterestRate,
                ProductType = a.ProductType,
                Status = a.Status,
                StatusTitle = AppEntity.GetStatusTitle(a.Status),
                CreatedAt = a.CreatedAt,
                DocumentsCount = a.Documents.Count
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ApplicationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Client)
            .Include(a => a.Documents)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (application == null) return null;

        return MapToDto(application);
    }

    public async Task<List<ApplicationSummaryDto>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _db.Applications
            .AsNoTracking()
            .Where(a => a.ClientId == clientId)
            .Include(a => a.Client)
            .Include(a => a.Documents)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ApplicationSummaryDto
            {
                Id = a.Id,
                ApplicationNumber = a.ApplicationNumber,
                ClientId = a.ClientId,
                ClientFullName = a.Client != null ? a.Client.FullName : string.Empty,
                ClientTaxNumber = a.Client != null ? a.Client.TaxNumber : string.Empty,
                LoanAmount = a.LoanAmount,
                LoanTermMonths = a.LoanTermMonths,
                InterestRate = a.InterestRate,
                ProductType = a.ProductType,
                Status = a.Status,
                StatusTitle = AppEntity.GetStatusTitle(a.Status),
                CreatedAt = a.CreatedAt,
                DocumentsCount = a.Documents.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationDto> CreateAsync(
        CreateApplicationDto dto,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var client = await _db.Clients.FindAsync(new object[] { dto.ClientId }, cancellationToken);
        if (client == null)
        {
            throw new DomainException($"Клиент с ID '{dto.ClientId}' не найден.");
        }

        var appNumber = await GenerateApplicationNumberAsync(cancellationToken);

        var application = new AppEntity
        {
            ApplicationNumber = appNumber,
            ClientId = dto.ClientId,
            LoanAmount = dto.LoanAmount,
            LoanTermMonths = dto.LoanTermMonths,
            InterestRate = dto.InterestRate,
            ProductType = dto.ProductType,
            Status = ApplicationStatus.New,
            CreatedByUserId = userId,
            CreatedByUserName = userName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        application.StatusHistory.Add(new ApplicationStatusHistory
        {
            ApplicationId = application.Id,
            OldStatus = ApplicationStatus.New,
            NewStatus = ApplicationStatus.New,
            Comment = "Заявка зарегистрирована в системе",
            ChangedByUserId = userId,
            ChangedByUserName = userName,
            ChangedAt = DateTimeOffset.UtcNow
        });

        _db.Applications.Add(application);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "CREATE_APPLICATION",
            "Application",
            application.Id.ToString(),
            $"Создана заявка № {application.ApplicationNumber} на сумму {application.LoanAmount:N2} ₽ для клиента {client.FullName}",
            cancellationToken: cancellationToken);

        await _notificationService.CreateNotificationAsync(
            null,
            UserRole.Manager,
            "Новая заявка на кредит",
            $"Зарегистрирована новая заявка {application.ApplicationNumber} ({application.LoanAmount:N0} ₽, {client.FullName})",
            application.Id,
            cancellationToken);

        return (await GetByIdAsync(application.Id, cancellationToken))!;
    }

    public async Task<ApplicationDto> UpdateAsync(
        UpdateApplicationDto dto,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var application = await _db.Applications
            .Include(a => a.Client)
            .FirstOrDefaultAsync(a => a.Id == dto.Id, cancellationToken);

        if (application == null)
        {
            throw new DomainException($"Заявка с ID '{dto.Id}' не найдена.");
        }

        if (application.Status != ApplicationStatus.New)
        {
            throw new DomainException("Редактирование параметров заявки возможно только в статусе 'Новая'.");
        }

        application.LoanAmount = dto.LoanAmount;
        application.LoanTermMonths = dto.LoanTermMonths;
        application.InterestRate = dto.InterestRate;
        application.ProductType = dto.ProductType;
        application.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "UPDATE_APPLICATION",
            "Application",
            application.Id.ToString(),
            $"Обновлены параметры заявки № {application.ApplicationNumber}",
            cancellationToken: cancellationToken);

        return (await GetByIdAsync(application.Id, cancellationToken))!;
    }

    public async Task<ApplicationDto> ChangeStatusAsync(
        Guid applicationId,
        ChangeStatusDto dto,
        string userId,
        string userName,
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        var validation = await _changeStatusValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var application = await _db.Applications
            .Include(a => a.Client)
            .Include(a => a.StatusHistory)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application == null)
        {
            throw new DomainException($"Заявка с ID '{applicationId}' не найдена.");
        }

        if ((dto.NewStatus == ApplicationStatus.Approved || dto.NewStatus == ApplicationStatus.Rejected) &&
            userRole == UserRole.Operator)
        {
            throw new DomainException("Операторы не имеют прав на утверждение или отклонение заявок. Это действие доступно только Менеджеру или Администратору.");
        }

        var oldStatus = application.Status;
        application.ChangeStatus(dto.NewStatus, dto.Comment, userId, userName);

        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "CHANGE_STATUS",
            "Application",
            application.Id.ToString(),
            $"Статус заявки № {application.ApplicationNumber} изменен с '{AppEntity.GetStatusTitle(oldStatus)}' на '{AppEntity.GetStatusTitle(dto.NewStatus)}'. Обоснование: {dto.Comment ?? "без комментария"}",
            cancellationToken: cancellationToken);

        var notificationTitle = $"Смена статуса заявки {application.ApplicationNumber}";
        var notificationText = $"Заявка {application.ApplicationNumber} переведена в статус '{AppEntity.GetStatusTitle(dto.NewStatus)}' пользователем {userName}";

        await _notificationService.CreateNotificationAsync(
            application.CreatedByUserId,
            null,
            notificationTitle,
            notificationText,
            application.Id,
            cancellationToken);

        return (await GetByIdAsync(application.Id, cancellationToken))!;
    }

    public async Task DeleteAsync(Guid id, string userId, string userName, CancellationToken cancellationToken = default)
    {
        var application = await _db.Applications
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (application == null) return;

        if (application.Status == ApplicationStatus.Approved)
        {
            throw new DomainException("Одобренную заявку нельзя удалить из реестра.");
        }

        _db.Applications.Remove(application);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "DELETE_APPLICATION",
            "Application",
            id.ToString(),
            $"Удалена заявка № {application.ApplicationNumber}",
            cancellationToken: cancellationToken);
    }

    private async Task<string> GenerateApplicationNumberAsync(CancellationToken cancellationToken)
    {
        var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
        var countToday = await _db.Applications
            .CountAsync(a => a.ApplicationNumber.StartsWith($"APP-{datePrefix}"), cancellationToken);

        return $"APP-{datePrefix}-{(countToday + 1):D4}";
    }

    private static ApplicationDto MapToDto(AppEntity a)
    {
        return new ApplicationDto
        {
            Id = a.Id,
            ApplicationNumber = a.ApplicationNumber,
            ClientId = a.ClientId,
            Client = a.Client == null ? null : new ClientDto
            {
                Id = a.Client.Id,
                FirstName = a.Client.FirstName,
                LastName = a.Client.LastName,
                MiddleName = a.Client.MiddleName,
                FullName = a.Client.FullName,
                BirthDate = a.Client.BirthDate,
                Age = a.Client.Age,
                PassportSeries = a.Client.PassportSeries,
                PassportNumber = a.Client.PassportNumber,
                TaxNumber = a.Client.TaxNumber,
                PhoneNumber = a.Client.PhoneNumber,
                Email = a.Client.Email,
                MonthlyIncome = a.Client.MonthlyIncome,
                EmploymentStatus = a.Client.EmploymentStatus,
                RegistrationAddress = a.Client.RegistrationAddress,
                CreditRating = a.Client.CreditRating,
                CreatedAt = a.Client.CreatedAt
            },
            LoanAmount = a.LoanAmount,
            LoanTermMonths = a.LoanTermMonths,
            InterestRate = a.InterestRate,
            ProductType = a.ProductType,
            Status = a.Status,
            StatusTitle = AppEntity.GetStatusTitle(a.Status),
            StatusComment = a.StatusComment,
            MonthlyPayment = a.MonthlyPayment,
            TotalPayment = a.TotalPayment,
            TotalOverpayment = a.TotalOverpayment,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            CreatedByUserId = a.CreatedByUserId,
            CreatedByUserName = a.CreatedByUserName,
            AssignedToUserId = a.AssignedToUserId,
            AssignedToUserName = a.AssignedToUserName,
            Documents = a.Documents.OrderByDescending(d => d.UploadedAt).Select(d => new DocumentDto
            {
                Id = d.Id,
                ApplicationId = d.ApplicationId,
                DocumentType = d.DocumentType,
                DocumentTypeName = d.DocumentType.ToString(),
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSizeBytes = d.FileSizeBytes,
                UploadedAt = d.UploadedAt,
                UploadedByUserName = d.UploadedByUserName
            }).ToList(),
            StatusHistory = a.StatusHistory.OrderByDescending(h => h.ChangedAt).Select(h => new StatusHistoryDto
            {
                Id = h.Id,
                OldStatus = h.OldStatus,
                OldStatusTitle = AppEntity.GetStatusTitle(h.OldStatus),
                NewStatus = h.NewStatus,
                NewStatusTitle = AppEntity.GetStatusTitle(h.NewStatus),
                Comment = h.Comment,
                ChangedByUserName = h.ChangedByUserName,
                ChangedAt = h.ChangedAt
            }).ToList()
        };
    }
}
