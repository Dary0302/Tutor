using Microsoft.EntityFrameworkCore;
using Tutor.Domain.Articles;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure.Configurations;

internal static class ArticlesMapping
{
    public static void Configure(ModelBuilder model)
    {
        var article = model.Entity<Article>();
        article.ToTable("articles");
        article.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        article.Property(x => x.AuthorId).HasColumnName("author");
        article.Property(x => x.Title).HasColumnName("title").HasMaxLength(255);
        article.Property(x => x.S3Key).HasColumnName("s3_key").HasMaxLength(255);
        article.HasOne<User>().WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}
