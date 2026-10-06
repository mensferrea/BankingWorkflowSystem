using BankingWorkflow.Application.DTOs.Clients;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Enums;
using BankingWorkflow.Domain.Exceptions;
using BankingWorkflow.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BankingWorkflow.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly BankingDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateClientDto> _createValidator;

    public ClientService(
        BankingDbContext db,
        IAuditService auditService,
        IValidator<CreateClientDto> createValidator)
    {
        _db = db;
        _auditService = auditService;
        _createValidator = createValidator;
    }

    public async Task<List<ClientDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Clients
            .AsNoTracking()
            .Include(c => c.Applications);

        IQueryable<Client> filtered = query;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            filtered = query.Where(c =>
                c.LastName.ToLower().Contains(s) ||
                c.FirstName.ToLower().Contains(s) ||
                (c.MiddleName != null && c.MiddleName.ToLower().Contains(s)) ||
                c.TaxNumber.Contains(s) ||
                c.PassportNumber.Contains(s) ||
                c.PhoneNumber.Contains(s));
        }

        var list = await filtered
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                MiddleName = c.MiddleName,
                FullName = c.FullName,
                BirthDate = c.BirthDate,
                Age = c.Age,
                PassportSeries = c.PassportSeries,
                PassportNumber = c.PassportNumber,
                TaxNumber = c.TaxNumber,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                MonthlyIncome = c.MonthlyIncome,
                EmploymentStatus = c.EmploymentStatus,
                RegistrationAddress = c.RegistrationAddress,
                CreditRating = c.CreditRating,
                CreatedAt = c.CreatedAt,
                ActiveApplicationsCount = c.Applications.Count(a => a.Status == ApplicationStatus.New || a.Status == ApplicationStatus.InProgress)
            })
            .ToListAsync(cancellationToken);

        return list;
    }

    public async Task<List<ClientLookupDto>> GetLookupAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                c.LastName.ToLower().Contains(s) ||
                c.FirstName.ToLower().Contains(s) ||
                c.TaxNumber.Contains(s));
        }

        return await query
            .OrderBy(c => c.LastName)
            .Take(50)
            .Select(c => new ClientLookupDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Passport = $"{c.PassportSeries} {c.PassportNumber}",
                TaxNumber = c.TaxNumber,
                MonthlyIncome = c.MonthlyIncome
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _db.Clients
            .AsNoTracking()
            .Include(c => c.Applications)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (client == null) return null;

        return new ClientDto
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            MiddleName = client.MiddleName,
            FullName = client.FullName,
            BirthDate = client.BirthDate,
            Age = client.Age,
            PassportSeries = client.PassportSeries,
            PassportNumber = client.PassportNumber,
            TaxNumber = client.TaxNumber,
            PhoneNumber = client.PhoneNumber,
            Email = client.Email,
            MonthlyIncome = client.MonthlyIncome,
            EmploymentStatus = client.EmploymentStatus,
            RegistrationAddress = client.RegistrationAddress,
            CreditRating = client.CreditRating,
            CreatedAt = client.CreatedAt,
            ActiveApplicationsCount = client.Applications.Count(a => a.Status == ApplicationStatus.New || a.Status == ApplicationStatus.InProgress)
        };
    }

    public async Task<ClientDto> CreateAsync(
        CreateClientDto dto,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var exists = await _db.Clients.AnyAsync(
            c => c.TaxNumber == dto.TaxNumber.Trim() ||
                (c.PassportSeries == dto.PassportSeries.Trim() && c.PassportNumber == dto.PassportNumber.Trim()),
            cancellationToken);

        if (exists)
        {
            throw new DomainException("Клиент с таким ИНН или паспортными данными уже зарегистрирован в системе.");
        }

        var client = new Client
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim(),
            BirthDate = dto.BirthDate,
            PassportSeries = dto.PassportSeries.Trim(),
            PassportNumber = dto.PassportNumber.Trim(),
            TaxNumber = dto.TaxNumber.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Email = dto.Email.Trim(),
            MonthlyIncome = dto.MonthlyIncome,
            EmploymentStatus = dto.EmploymentStatus,
            RegistrationAddress = dto.RegistrationAddress.Trim(),
            CreditRating = CalculateInitialRating(dto.MonthlyIncome, dto.EmploymentStatus)
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "CREATE_CLIENT",
            "Client",
            client.Id.ToString(),
            $"Зарегистрирован новый клиент {client.FullName}, ИНН {client.TaxNumber}",
            cancellationToken: cancellationToken);

        return (await GetByIdAsync(client.Id, cancellationToken))!;
    }

    public async Task<ClientDto> UpdateAsync(
        UpdateClientDto dto,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);
        if (client == null)
        {
            throw new DomainException($"Клиент с ID '{dto.Id}' не найден.");
        }

        var existsDuplicate = await _db.Clients.AnyAsync(
            c => c.Id != dto.Id &&
                (c.TaxNumber == dto.TaxNumber.Trim() ||
                (c.PassportSeries == dto.PassportSeries.Trim() && c.PassportNumber == dto.PassportNumber.Trim())),
            cancellationToken);

        if (existsDuplicate)
        {
            throw new DomainException("Другой клиент с таким ИНН или паспортом уже зарегистрирован.");
        }

        client.UpdateDetails(
            dto.FirstName,
            dto.LastName,
            dto.MiddleName,
            dto.BirthDate,
            dto.PassportSeries,
            dto.PassportNumber,
            dto.TaxNumber,
            dto.PhoneNumber,
            dto.Email,
            dto.MonthlyIncome,
            dto.EmploymentStatus,
            dto.RegistrationAddress,
            dto.CreditRating);

        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "UPDATE_CLIENT",
            "Client",
            client.Id.ToString(),
            $"Обновлены анкетные данные клиента {client.FullName}",
            cancellationToken: cancellationToken);

        return (await GetByIdAsync(client.Id, cancellationToken))!;
    }

    public async Task DeleteAsync(Guid id, string userId, string userName, CancellationToken cancellationToken = default)
    {
        var client = await _db.Clients
            .Include(c => c.Applications)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (client == null) return;

        if (client.Applications.Any())
        {
            throw new DomainException("Невозможно удалить клиента, у которого есть оформленные заявки.");
        }

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            userName,
            "DELETE_CLIENT",
            "Client",
            id.ToString(),
            $"Удалена карточка клиента {client.FullName}",
            cancellationToken: cancellationToken);
    }

    private static CreditRating CalculateInitialRating(decimal income, EmploymentStatus employment)
    {
        if (employment == EmploymentStatus.Unemployed) return CreditRating.Poor;
        if (income >= 150_000) return CreditRating.Excellent;
        if (income >= 70_000) return CreditRating.Good;
        if (income >= 30_000) return CreditRating.Fair;
        return CreditRating.Poor;
    }
}
