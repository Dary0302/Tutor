using Microsoft.EntityFrameworkCore;
using Tutor.Domain.Applications;

namespace Tutor.Infrastructure.Configurations;

internal static class AppMapping
{
    public static void Configure(ModelBuilder model)
    {
        var article = model.Entity<App>();
        article.ToTable("applications");
        article.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        article.Property(x => x.StudentId).HasColumnName("student").ValueGeneratedNever();
        article.Property(x => x.MentorId).HasColumnName("mentor").ValueGeneratedNever();
        article.Property(x => x.MentorsAproove).HasColumnName("mentorsAproove");
        article.Property(x => x.TutorsAproove).HasColumnName("tutorsAproove");
    }
}