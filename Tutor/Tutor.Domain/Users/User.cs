namespace Tutor.Domain.Users;

public sealed class User
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public required UserRole Role { get; init; }
    public int TotalBalance { get; init; }
    public int TotalPoints { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public byte[]? RefreshTokenHash { get; init; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; init; }
}
