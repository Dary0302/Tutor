using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Carbon.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tutor.Domain.Articles;
using DeleteObjectRequest = Amazon.S3.Model.DeleteObjectRequest;
using PutObjectRequest = Amazon.S3.Model.PutObjectRequest;

namespace Tutor.Infrastructure.Articles;

public sealed class ArticleStorage : IArticleStorage
{
    private readonly IAmazonS3 _s3;
    private readonly StorageOptions _options;
    private readonly ILogger<ArticleStorage> _logger;

    public ArticleStorage(
        IAmazonS3 s3,
        IOptions<StorageOptions> options,
        ILogger<ArticleStorage> logger)
    {
        _s3 = s3;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        Guid articleId, string markdown, CancellationToken ct = default)
    {
        var key = BuildKey(articleId);

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            ContentBody = markdown,
            ContentType = "text/markdown; charset=utf-8",
        };

        try
        {
            await _s3.PutObjectAsync(request, ct);
            _logger.LogInformation("Uploaded article {ArticleId} to S3 as {Key}", articleId, key);
            return key;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 upload failed for article {ArticleId}", articleId);
            throw new StorageException("Не удалось загрузить статью в хранилище", ex);
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
        };

        try
        {
            await _s3.DeleteObjectAsync(request, ct);
            _logger.LogInformation("Deleted S3 object {Key}", key);
        }
        catch (AmazonS3Exception ex)
        {
            // Если объекта уже нет — не считаем это ошибкой
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("S3 object {Key} not found on delete", key);
                return;
            }
            _logger.LogError(ex, "S3 delete failed for {Key}", key);
            throw new StorageException("Не удалось удалить статью из хранилища", ex);
        }
    }
    
    public string GetPreSignedUrl(string s3Key)
    {
        if (string.IsNullOrWhiteSpace(s3Key)) return null;

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = s3Key,
            Verb = HttpVerb.GET,
            Protocol = Protocol.HTTPS,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        return _s3.GetPreSignedURL(request);
    }

    private static string BuildKey(Guid articleId) => $"articles/{articleId:N}.md";
}