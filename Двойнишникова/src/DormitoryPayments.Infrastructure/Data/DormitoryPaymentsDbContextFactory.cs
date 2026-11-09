using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DormitoryPayments.Infrastructure.Data;

public sealed class DormitoryPaymentsDbContextFactory
    : IDesignTimeDbContextFactory<DormitoryPaymentsDbContext>
{
    public const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=DormitoryPaymentsDb;" +
        "Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public DormitoryPaymentsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DORMITORY_DB_CONNECTION")
            ?? DefaultConnectionString;

        var options = new DbContextOptionsBuilder<DormitoryPaymentsDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(DormitoryPaymentsDbContext).Assembly.FullName))
            .Options;

        return new DormitoryPaymentsDbContext(options);
    }
}
