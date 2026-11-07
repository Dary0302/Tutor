namespace Tutor.Domain.Articles;

public interface IArticleStorage
{
    Task<string> UploadAsync(Guid articleId, string content, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
    string GetPreSignedUrl(string s3Key);
}