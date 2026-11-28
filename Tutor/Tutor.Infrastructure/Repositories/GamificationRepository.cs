using Microsoft.EntityFrameworkCore;
using Tutor.Application.Gamification;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Users;
using Tutor.Infrastructure.Repositories.Models;

namespace Tutor.Infrastructure.Repositories;

internal sealed class GamificationRepository(IDbContextFactory<TutorDbContext> factory) : IGamificationRepository
{
    public async Task<IReadOnlyList<LeaderboardScore>> GetLeadersAsync(UserRole role, int limit,
        GamificationOptions options, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await LeaderboardQuery(db, role, limit, options).ToListAsync(ct);
    }

    public async Task<GamificationProgress?> GetProgressAsync(Guid userId, UserRole role,
        GamificationOptions options, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await ProfileQuery(db, userId, role, options).SingleOrDefaultAsync(ct);
    }

    internal static IQueryable<LeaderboardScore> LeaderboardQuery(TutorDbContext db, UserRole role, int limit,
        GamificationOptions options)
    {
        var leaders = Scores(db, role, options).Where(score => score.Points > 0)
            .OrderByDescending(score => score.Points).ThenBy(score => score.UserId).Take(limit);
        return from score in leaders
               join questionnaire in db.Set<Questionnaire>() on score.UserId equals questionnaire.UserId into questionnaires
               from questionnaire in questionnaires.DefaultIfEmpty()
               orderby score.Points descending, score.UserId
               select new LeaderboardScore(score.UserId, questionnaire == null ? null : questionnaire.FirstName,
                   questionnaire == null ? null : questionnaire.LastName, score.Points);
    }

    internal static IQueryable<GamificationProgress> ProfileQuery(TutorDbContext db, Guid userId, UserRole role,
        GamificationOptions options)
    {
        var scores = Scores(db, role, options);
        return scores.Where(score => score.UserId == userId)
            .Select(score => new GamificationProgress(score.Points,
                score.Points > 0 ? scores.LongCount(other => other.Points > score.Points) + 1 : null,
                score.Sessions, score.Tasks, score.Goals, score.HasQuestionnaire));
    }

    private static IQueryable<GamificationScore> Scores(TutorDbContext db, UserRole role, GamificationOptions options)
    {
        var isStudent = role == UserRole.Student;
        var sessionPoints = isStudent ? options.Student.SessionPoints : options.Mentor.SessionPoints;
        var goalPoints = isStudent ? options.Student.GoalPoints : options.Mentor.StudentGoalPoints;
        var questionnairePoints = isStudent ? options.Student.QuestionnairePoints : options.Mentor.QuestionnairePoints;
        var taskPoints = options.Student.TaskPoints;
        var pairs = db.Set<MentorshipPair>().Select(pair => new
        {
            pair.Id, UserId = isStudent ? pair.Request.StudentId : pair.Request.TeacherId
        });
        var sessions = from session in db.Set<MentoringSession>()
                       join pair in pairs on session.PairId equals pair.Id
                       where session.Status == SessionStatus.Completed
                       select pair.UserId;
        var goals = from goal in db.Set<LearningGoal>()
                    join pair in pairs on goal.PairId equals pair.Id
                    where goal.Status == GoalStatus.Completed
                    select pair.UserId;
        var tasks = from goal in db.Set<LearningGoal>()
                    join pair in pairs on goal.PairId equals pair.Id
                    from task in goal.Tasks
                    where goal.Status != GoalStatus.Cancelled && task.Status == GoalTaskStatus.Completed
                    select pair.UserId;

        return from user in db.Users.AsNoTracking()
               where user.Role == role
               let sessionCount = sessions.LongCount(userId => userId == user.Id)
               let goalCount = goals.LongCount(userId => userId == user.Id)
               let taskCount = isStudent ? tasks.LongCount(userId => userId == user.Id) : 0L
               let hasQuestionnaire = db.Set<Questionnaire>().Any(q => q.UserId == user.Id)
               select new GamificationScore
               {
                   UserId = user.Id, Sessions = sessionCount, Tasks = taskCount, Goals = goalCount,
                   HasQuestionnaire = hasQuestionnaire,
                   Points = sessionCount * sessionPoints + goalCount * goalPoints + taskCount * taskPoints
                       + (hasQuestionnaire ? questionnairePoints : 0)
               };
    }
}
