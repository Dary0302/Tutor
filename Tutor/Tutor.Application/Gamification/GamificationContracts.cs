using Tutor.Domain.Users;

namespace Tutor.Application.Gamification;

public sealed class LeaderboardRequest
{
    public const int MaxLimit = 100;
    public int Limit { get; init; } = 10;
}

public sealed record LeaderboardEntryDto(Guid UserId, string? FirstName, string? LastName, long Points, long Rank);

public sealed record AchievementDto(string Code, string Title, string Description);

public sealed record GamificationProfileDto(long Points, long? Rank, IReadOnlyList<AchievementDto> Achievements);

public sealed record GamificationProgress(long Points, long? Rank, long Sessions, long Tasks, long Goals,
    bool HasQuestionnaire);

public sealed record LeaderboardScore(Guid UserId, string? FirstName, string? LastName, long Points);

public interface IGamificationRepository
{
    Task<IReadOnlyList<LeaderboardScore>> GetLeadersAsync(UserRole role, int limit, GamificationOptions options,
        CancellationToken ct);
    Task<GamificationProgress?> GetProgressAsync(Guid userId, UserRole role, GamificationOptions options,
        CancellationToken ct);
}
