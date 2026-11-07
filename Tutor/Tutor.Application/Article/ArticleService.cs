using System.ComponentModel.DataAnnotations;
using Tutor.Domain.Articles;

namespace Tutor.Application.Article;

public class ArticleService(IArticleRepository articles, IArticleStorage storage)
{
    public async Task<ArticleDetailDto> CreateArticle(string title, string content, Guid authorId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Title is required");
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Content is required");

        var articleId = Guid.NewGuid();
        
        var s3Key = await storage.UploadAsync(articleId, content, cancellationToken);
        
        var article = new Domain.Articles.Article
        {
            Id = articleId,
            Title = title,
            AuthorId = authorId,
            S3Key = s3Key,
        };
        
        await articles.TryCreateAsync(article, cancellationToken);
        
        return new ArticleDetailDto(articleId, null, null, null, s3Key);
    }
    
    public async Task<bool> UpdateAsync(
        Guid id, string? content, CancellationToken cancellationToken)
    {
        var article = await articles.GetByIdAsync(id, cancellationToken);
        if (article is null) return false;
        
        if (content is not null)
        {
            await storage.UploadAsync(article.Id, content, cancellationToken);
        }

        return true;
    }
    
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var article = await articles.GetByIdAsync(id, cancellationToken);
        if (article is null) return false;

        await storage.DeleteAsync(article.S3Key, cancellationToken); 
        await articles.TryDeleteAsync(article, cancellationToken);
        
        return true;
    }

    public async Task<ListTitlesResponse?> GetAllTitels(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var listArticles = await articles.GetAllAsync(cancellationToken);
        
        var totalCount = listArticles.Count;
        var start = (pageNumber - 1) * pageSize;
        if (start > totalCount || start <= 0) 
            return null;

        int end;
        if (start + pageSize > totalCount)
            end = totalCount;
        else
            end = start + pageSize;
        var listTitles = new List<TitleResponse>();
        
        for (var i = start; i < end; i++)
        {
            listTitles.Add(new TitleResponse(listArticles[i].Title, listArticles[i].Id, listArticles[i].AuthorId));
        }
        
        return new ListTitlesResponse(listTitles, totalCount, pageNumber, pageSize);
    }

    public async Task<ArticleDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var article = await articles.GetByIdAsync(id, cancellationToken);

        if (article == null)
            return null;

        var contentUrl = storage.GetPreSignedUrl(article.S3Key);

        return new ArticleDetailDto(article.Id, article.Title, contentUrl, article.AuthorId, article.S3Key);
    }
}