namespace Tutor.Domain.Billing;

public sealed class AccountTransaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public int PointsAmount { get; set; }
    public int BalanceAmount { get; set; }
    public TransactionReason Reason { get; set; }
    public required string Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
