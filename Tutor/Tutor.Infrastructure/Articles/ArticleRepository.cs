using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tutor.Domain.Articles;

namespace Tutor.Infrastructure.Articles;

internal sealed class ArticleRepository(TutorDbContext db): IArticleRepository
{
    private IArticleRepository articleRepositoryImplementation;
    public  Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> TryCreateAsync(Article article, CancellationToken cancellationToken)
    {
        var entry = db.Articles.Add(article);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) 
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }

    public async Task<bool> TryDeleteAsync(Article article, CancellationToken cancellationToken)
    {
        var entry = db.Articles.Remove(article);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) 
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }

    public async Task<List<Article>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.Articles.AsNoTracking().ToListAsync(cancellationToken);
    }
}