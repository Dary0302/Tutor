namespace Tutor.Domain.Articles;

public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> TryCreateAsync(Article article, CancellationToken cancellationToken);
    Task<bool> TryDeleteAsync(Article article, CancellationToken cancellationToken);
    Task<List<Article>> GetAllAsync(CancellationToken cancellationToken);
}