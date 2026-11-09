using Microsoft.EntityFrameworkCore;

namespace DormitoryPayments.Infrastructure.Data;

public sealed class DatabaseBootstrapper(
    IDbContextFactory<DormitoryPaymentsDbContext> contextFactory,
    DatabaseSeeder seeder)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
        await seeder.SeedAsync(cancellationToken);
    }
}
