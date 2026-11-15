using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tutor.Application.Goals;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;

namespace Tutor.Infrastructure.Repositories;

internal sealed class GoalWriteRepository(TutorDbContext db, IDbContextTransaction transaction,
    MentorshipPair pair, LearningGoal? goal) : IGoalWriteRepository
{
    public GoalPairContext Pair { get; } = new(pair.Id, pair.Request.TeacherId, pair.Request.StudentId, pair.Status);
    public LearningGoal? Goal { get; } = goal;

    public Task<Skill?> FindSkillAsync(Guid skillId, CancellationToken ct) =>
        db.Set<Skill>().SingleOrDefaultAsync(s => s.Id == skillId, ct);

    public void Add(LearningGoal newGoal) => db.Add(newGoal);

    public async Task SaveAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        try { await transaction.DisposeAsync(); }
        finally { await db.DisposeAsync(); }
    }
}
