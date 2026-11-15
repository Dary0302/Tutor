namespace Tutor.Application.Goals;

public interface IGoalRepository
{
    Task<GoalPage<GoalDto>> ListAsync(Guid userId, GoalListRequest request, CancellationToken ct);
    Task<GoalDto?> GetAsync(Guid userId, Guid goalId, CancellationToken ct);
    Task<GoalPage<GoalStudentDto>> GetStudentsAsync(Guid mentorId, GoalPageRequest request, CancellationToken ct);
    Task<GoalPage<GoalSkillDto>> GetSkillsAsync(GoalLookupRequest request, CancellationToken ct);
    Task<IReadOnlyList<SkillProgressCounts>> GetProgressAsync(Guid userId, Guid? pairId, CancellationToken ct);
    Task<IGoalWriteRepository?> EditPairAsync(Guid userId, Guid pairId, CancellationToken ct);
    Task<IGoalWriteRepository?> EditGoalAsync(Guid userId, Guid goalId, CancellationToken ct);
}
