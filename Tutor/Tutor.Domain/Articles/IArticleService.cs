namespace Tutor.Domain.Articles;

public interface IArticleService
{
    Task<ArticleDetailDto> CreateArticle(string title, string content, Guid authorId,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(Guid id, string? content, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<ListTitlesResponse?> GetAllTitels(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<ArticleDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}