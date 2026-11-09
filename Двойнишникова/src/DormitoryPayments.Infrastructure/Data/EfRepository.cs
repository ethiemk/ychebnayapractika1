using DormitoryPayments.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryPayments.Infrastructure.Data;

public sealed class EfRepository<TEntity, TKey>(
    IDbContextFactory<DormitoryPaymentsDbContext> contextFactory)
    : IRepository<TEntity, TKey>
    where TEntity : class
{
    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Set<TEntity>().Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Set<TEntity>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
