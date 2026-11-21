namespace Tutor.Application.Goals;

public interface IGoalProgressService
{
    Task<IReadOnlyList<SkillProgressDto>> GetAsync(Guid userId, Guid? pairId, CancellationToken ct);
}
