namespace Tutor.Domain.Articles;

public record ArticleDetailDto(
    Guid Id,
    string Title,
    string Content,
    Guid AuthorId,
    string S3Key);