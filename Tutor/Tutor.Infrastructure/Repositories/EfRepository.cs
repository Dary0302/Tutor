using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tutor.Domain.Repositories;

namespace Tutor.Infrastructure.Repositories;

internal sealed class EfRepository<TEntity, TKey>(
    IDbContextFactory<TutorDbContext> contexts,
    Func<TKey, object?[]> keyValues) : IRepository<TEntity, TKey> where TEntity : class
{
    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        return await db.Set<TEntity>().FindAsync(keyValues(id), cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? filter,
        CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var query = db.Set<TEntity>().AsNoTracking();
        if (filter is not null)
            query = query.Where(filter);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        db.Set<TEntity>().Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        db.Entry(entity).State = EntityState.Modified;
        return await SaveExistingAsync(db, cancellationToken);
    }

    public async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var entity = await db.Set<TEntity>().FindAsync(keyValues(id), cancellationToken);
        if (entity is null)
            return false;

        db.Remove(entity);
        return await SaveExistingAsync(db, cancellationToken);
    }

    private static async Task<bool> SaveExistingAsync(TutorDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // With no version column, zero affected rows means the record no longer exists.
            return false;
        }
    }
}
