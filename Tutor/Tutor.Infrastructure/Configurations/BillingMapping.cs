using Microsoft.EntityFrameworkCore;
using Tutor.Domain.Billing;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure.Configurations;

internal static class BillingMapping
{
    public static void Configure(ModelBuilder model)
    {
        var entity = model.Entity<AccountTransaction>();
        entity.ToTable("transactions");
        entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(x => x.PointsAmount).HasColumnName("points_amount");
        entity.Property(x => x.BalanceAmount).HasColumnName("balance_amount");
        entity.Property(x => x.Reason).HasColumnName("reason").HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.Description).HasColumnName("description");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
