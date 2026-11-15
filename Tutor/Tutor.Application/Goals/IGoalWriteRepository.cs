using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Goals;

public interface IGoalWriteRepository : IAsyncDisposable
{
    GoalPairContext Pair { get; }
    LearningGoal? Goal { get; }
    Task<Skill?> FindSkillAsync(Guid skillId, CancellationToken ct);
    void Add(LearningGoal goal);
    Task SaveAsync(CancellationToken ct);
}

public sealed record GoalPairContext(Guid Id, Guid MentorId, Guid StudentId, PairStatus Status);
