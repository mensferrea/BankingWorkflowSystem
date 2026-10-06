using BankingWorkflow.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AppEntity = BankingWorkflow.Domain.Entities.Application;

namespace BankingWorkflow.Infrastructure.Data;

public class BankingDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<AppEntity> Applications => Set<AppEntity>();
    public DbSet<AttachedDocument> AttachedDocuments => Set<AttachedDocument>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.PassportSeries).HasMaxLength(10).IsRequired();
            entity.Property(e => e.PassportNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TaxNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.Property(e => e.MonthlyIncome).HasPrecision(18, 2);
            entity.Property(e => e.RegistrationAddress).HasMaxLength(500);

            entity.HasIndex(e => e.TaxNumber);
            entity.HasIndex(e => new { e.PassportSeries, e.PassportNumber });
        });

        builder.Entity<AppEntity>(entity =>
        {
            entity.ToTable("applications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApplicationNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LoanAmount).HasPrecision(18, 2);
            entity.Property(e => e.InterestRate).HasPrecision(5, 2);
            entity.Property(e => e.StatusComment).HasMaxLength(1000);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Applications)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ApplicationNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        builder.Entity<AttachedDocument>(entity =>
        {
            entity.ToTable("attached_documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Application)
                .WithMany(a => a.Documents)
                .HasForeignKey(e => e.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ApplicationId);
        });

        builder.Entity<ApplicationStatusHistory>(entity =>
        {
            entity.ToTable("application_status_histories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.ChangedByUserName).HasMaxLength(150);

            entity.HasOne(e => e.Application)
                .WithMany(a => a.StatusHistory)
                .HasForeignKey(e => e.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ApplicationId);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Details).HasMaxLength(2000);

            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EntityName);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TargetRole);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
