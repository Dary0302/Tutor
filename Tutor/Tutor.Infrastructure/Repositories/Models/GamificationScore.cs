namespace Tutor.Infrastructure.Repositories.Models;

internal sealed class GamificationScore
{
    public Guid UserId { get; init; }
    public long Points { get; init; }
    public long Sessions { get; init; }
    public long Tasks { get; init; }
    public long Goals { get; init; }
    public bool HasQuestionnaire { get; init; }
}
