using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Auth.Domain.Entities;

namespace Auth.Infrastructure.EntitiesConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("Password")
            .IsRequired();

        builder.Property(x => x.RefreshToken)
            .HasColumnName("RefreshToken");

        builder.Property(x => x.Role)
            .HasColumnName("Role")
            .HasConversion<string>() // Сохраняем в БД как строку (например, "Admin")
            .HasMaxLength(50)
            .IsRequired();
    }
}