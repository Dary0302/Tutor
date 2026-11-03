namespace Tutor.Domain.Articles;

public sealed class Article
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid AuthorId { get; set; }
    public required string Title { get; set; }
    public required string S3Key { get; set; }
}
