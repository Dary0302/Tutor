using Microsoft.EntityFrameworkCore;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure;

internal sealed class TutorDbContext(DbContextOptions<TutorDbContext> options) : DbContext(options)
{
    internal const string EmailIndex = "users_email_unique";

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.ToTable("Users");
        user.HasKey(u => u.Id);
        user.Property(u => u.Id).ValueGeneratedNever();
        user.Property(u => u.Email).HasColumnName("Name").HasMaxLength(100);
        user.Property(u => u.PasswordHash).HasColumnName("Password");
        user.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
        user.HasIndex(u => u.Email).IsUnique().HasDatabaseName(EmailIndex);
        user.HasIndex(u => u.RefreshTokenHash).IsUnique().HasDatabaseName("users_refresh_token_unique");
        user.HasQueryFilter(u => u.DeletedAt == null);
    }
}
