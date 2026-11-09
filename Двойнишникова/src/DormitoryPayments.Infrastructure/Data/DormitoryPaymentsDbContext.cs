using DormitoryPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DormitoryPayments.Infrastructure.Data;

public sealed class DormitoryPaymentsDbContext(DbContextOptions<DormitoryPaymentsDbContext> options)
    : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Dormitory> Dormitories => Set<Dormitory>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Accommodation> Accommodations => Set<Accommodation>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<Accrual> Accruals => Set<Accrual>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureRole(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureStudent(modelBuilder);
        ConfigureDormitory(modelBuilder);
        ConfigureRoom(modelBuilder);
        ConfigureAccommodation(modelBuilder);
        ConfigureTariff(modelBuilder);
        ConfigureAccrual(modelBuilder);
        ConfigurePayment(modelBuilder);
        ConfigurePaymentAllocation(modelBuilder);
        ConfigureAuditLog(modelBuilder);
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Role>();
        entity.ToTable("Roles");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Name).HasMaxLength(50).IsRequired();
        entity.HasIndex(item => item.Name).IsUnique();
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();
        entity.ToTable("Users");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Login).HasMaxLength(50).IsRequired();
        entity.Property(item => item.PasswordHash).HasMaxLength(512).IsRequired();
        entity.Property(item => item.DisplayName).HasMaxLength(150).IsRequired();
        entity.Property(item => item.CreatedAtUtc).HasPrecision(0);
        entity.HasIndex(item => item.Login).IsUnique();
        entity.HasOne(item => item.Role)
            .WithMany(item => item.Users)
            .HasForeignKey(item => item.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureStudent(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Student>();
        entity.ToTable("Students");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.StudentNumber).HasMaxLength(30).IsRequired();
        entity.Property(item => item.LastName).HasMaxLength(80).IsRequired();
        entity.Property(item => item.FirstName).HasMaxLength(80).IsRequired();
        entity.Property(item => item.Patronymic).HasMaxLength(80);
        entity.Property(item => item.GroupName).HasMaxLength(30).IsRequired();
        entity.Property(item => item.Phone).HasMaxLength(30);
        entity.Property(item => item.Email).HasMaxLength(150);
        entity.HasIndex(item => item.StudentNumber).IsUnique();
        entity.HasIndex(item => new { item.LastName, item.FirstName });
    }

    private static void ConfigureDormitory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Dormitory>();
        entity.ToTable("Dormitories");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Name).HasMaxLength(100).IsRequired();
        entity.Property(item => item.Address).HasMaxLength(250).IsRequired();
        entity.HasIndex(item => item.Name).IsUnique();
    }

    private static void ConfigureRoom(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Room>();
        entity.ToTable(
            "Rooms",
            table => table.HasCheckConstraint("CK_Rooms_Capacity", "[Capacity] > 0"));
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Number).HasMaxLength(20).IsRequired();
        entity.Property(item => item.Gender).HasConversion<byte>();
        entity.HasIndex(item => new { item.DormitoryId, item.Number }).IsUnique();
        entity.HasOne(item => item.Dormitory)
            .WithMany(item => item.Rooms)
            .HasForeignKey(item => item.DormitoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAccommodation(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Accommodation>();
        entity.ToTable(
            "Accommodations",
            table => table.HasCheckConstraint(
                "CK_Accommodations_Dates",
                "[EndDate] IS NULL OR [EndDate] >= [StartDate]"));
        entity.HasKey(item => item.Id);
        entity.Property(item => item.StartDate).HasColumnType("date");
        entity.Property(item => item.EndDate).HasColumnType("date");
        entity.HasIndex(item => new { item.StudentId, item.StartDate });
        entity.HasIndex(item => new { item.RoomId, item.StartDate, item.EndDate });
        entity.HasOne(item => item.Student)
            .WithMany(item => item.Accommodations)
            .HasForeignKey(item => item.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(item => item.Room)
            .WithMany(item => item.Accommodations)
            .HasForeignKey(item => item.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureTariff(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Tariff>();
        entity.ToTable(
            "Tariffs",
            table =>
            {
                table.HasCheckConstraint("CK_Tariffs_Amount", "[Amount] > 0");
                table.HasCheckConstraint("CK_Tariffs_DueDay", "[DueDay] BETWEEN 1 AND 28");
                table.HasCheckConstraint(
                    "CK_Tariffs_Dates",
                    "[EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]");
            });
        entity.HasKey(item => item.Id);
        entity.Property(item => item.ServiceType).HasMaxLength(100).IsRequired();
        entity.Property(item => item.Amount).HasPrecision(18, 2);
        entity.Property(item => item.EffectiveFrom).HasColumnType("date");
        entity.Property(item => item.EffectiveTo).HasColumnType("date");
        entity.HasIndex(item => new { item.ServiceType, item.EffectiveFrom });
    }

    private static void ConfigureAccrual(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Accrual>();
        entity.ToTable(
            "Accruals",
            table => table.HasCheckConstraint("CK_Accruals_Amount", "[Amount] > 0"));
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Period).HasColumnType("date");
        entity.Property(item => item.DueDate).HasColumnType("date");
        entity.Property(item => item.Amount).HasPrecision(18, 2);
        entity.Property(item => item.Status).HasConversion<byte>();
        entity.Property(item => item.CreatedAtUtc).HasPrecision(0);
        entity.HasIndex(item => new { item.StudentId, item.TariffId, item.Period }).IsUnique();
        entity.HasIndex(item => new { item.Period, item.StudentId });
        entity.HasOne(item => item.Student)
            .WithMany(item => item.Accruals)
            .HasForeignKey(item => item.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(item => item.Tariff)
            .WithMany(item => item.Accruals)
            .HasForeignKey(item => item.TariffId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePayment(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Payment>();
        entity.ToTable(
            "Payments",
            table => table.HasCheckConstraint("CK_Payments_Amount", "[Amount] > 0"));
        entity.HasKey(item => item.Id);
        entity.Property(item => item.PaymentDate).HasColumnType("date");
        entity.Property(item => item.Amount).HasPrecision(18, 2);
        entity.Property(item => item.Method).HasConversion<byte>();
        entity.Property(item => item.DocumentNumber).HasMaxLength(60).IsRequired();
        entity.Property(item => item.Note).HasMaxLength(500);
        entity.Property(item => item.CreatedAtUtc).HasPrecision(0);
        entity.HasIndex(item => item.DocumentNumber).IsUnique();
        entity.HasIndex(item => new { item.PaymentDate, item.StudentId });
        entity.HasOne(item => item.Student)
            .WithMany(item => item.Payments)
            .HasForeignKey(item => item.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePaymentAllocation(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PaymentAllocation>();
        entity.ToTable(
            "PaymentAllocations",
            table => table.HasCheckConstraint("CK_PaymentAllocations_Amount", "[Amount] > 0"));
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Amount).HasPrecision(18, 2);
        entity.HasIndex(item => new { item.PaymentId, item.AccrualId }).IsUnique();
        entity.HasOne(item => item.Payment)
            .WithMany(item => item.Allocations)
            .HasForeignKey(item => item.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(item => item.Accrual)
            .WithMany(item => item.Allocations)
            .HasForeignKey(item => item.AccrualId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuditLog>();
        entity.ToTable("AuditLogs");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.TimestampUtc).HasPrecision(0);
        entity.Property(item => item.Level).HasMaxLength(30).IsRequired();
        entity.Property(item => item.Action).HasMaxLength(120).IsRequired();
        entity.Property(item => item.Details).HasMaxLength(2000).IsRequired();
        entity.HasIndex(item => item.TimestampUtc);
        entity.HasOne(item => item.User)
            .WithMany(item => item.AuditLogs)
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
