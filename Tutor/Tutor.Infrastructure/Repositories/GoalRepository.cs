using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tutor.Application.Goals;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure.Repositories;

internal sealed class GoalRepository(IDbContextFactory<TutorDbContext> factory, IMapper mapper) : IGoalRepository
{
    public async Task<GoalPage<GoalDto>> ListAsync(Guid userId, GoalListRequest request, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var goals = FilterGoals(db, userId, request);
        var total = await goals.CountAsync(ct);
        var items = Offset(request) >= total ? []
            : await GoalPageQuery(goals, request, mapper.ConfigurationProvider).ToListAsync(ct);
        return new GoalPage<GoalDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<GoalDto?> GetAsync(Guid userId, Guid goalId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await VisibleGoals(db, userId).Where(g => g.Id == goalId && g.Status != GoalStatus.Cancelled)
            .ProjectTo<GoalDto>(mapper.ConfigurationProvider).SingleOrDefaultAsync(ct);
    }

    public async Task<GoalPage<GoalStudentDto>> GetStudentsAsync(Guid mentorId, GoalPageRequest request, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var students = StudentQuery(db, mentorId).OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ThenBy(s => s.PairId);
        return await ReadPageAsync(students, request, ct);
    }

    public async Task<GoalPage<GoalSkillDto>> GetSkillsAsync(GoalLookupRequest request, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var skills = db.Set<Skill>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var pattern = "%" + request.Query.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            skills = skills.Where(s => EF.Functions.ILike(s.Name, pattern, "\\"));
        }
        return await ReadPageAsync(skills.OrderBy(s => s.Name).ThenBy(s => s.Id)
            .ProjectTo<GoalSkillDto>(mapper.ConfigurationProvider), request, ct);
    }

    public async Task<IReadOnlyList<SkillProgressCounts>> GetProgressAsync(Guid userId, Guid? pairId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await ProgressQuery(db, userId, pairId).ToListAsync(ct);
    }

    public Task<IGoalWriteRepository?> EditPairAsync(Guid userId, Guid pairId, CancellationToken ct) =>
        OpenEditAsync(userId, pairId, null, ct);

    public Task<IGoalWriteRepository?> EditGoalAsync(Guid userId, Guid goalId, CancellationToken ct) =>
        OpenEditAsync(userId, null, goalId, ct);

    private async Task<IGoalWriteRepository?> OpenEditAsync(Guid userId, Guid? pairId, Guid? goalId, CancellationToken ct)
    {
        var db = await factory.CreateDbContextAsync(ct);
        IDbContextTransaction? transaction = null;
        var resourcesTransferred = false;
        try
        {
            pairId = goalId.HasValue
                ? await VisibleGoals(db, userId).Where(g => g.Id == goalId).Select(g => (Guid?)g.PairId).SingleOrDefaultAsync(ct)
                : await VisiblePairs(db, userId).Where(p => p.Id == pairId).Select(p => (Guid?)p.Id).SingleOrDefaultAsync(ct);
            if (!pairId.HasValue)
                return null;

            transaction = await db.Database.BeginTransactionAsync(ct);
            var locked = await db.Set<MentorshipPair>()
                .FromSql($"SELECT * FROM pairs WHERE id = {pairId.Value} FOR UPDATE").SingleOrDefaultAsync(ct);
            if (locked is null)
                return null;

            var pair = await VisiblePairs(db, userId).AsTracking().Include(p => p.Request)
                .SingleOrDefaultAsync(p => p.Id == pairId, ct);
            if (pair is null)
                return null;
            LearningGoal? goal = null;
            if (goalId.HasValue)
            {
                goal = await db.Set<LearningGoal>().Include(g => g.Skill).Include(g => g.Tasks)
                    .SingleOrDefaultAsync(g => g.Id == goalId && g.PairId == pairId, ct);
                if (goal is null)
                    return null;
            }
            var writeRepository = new GoalWriteRepository(db, transaction, pair, goal);
            resourcesTransferred = true;
            return writeRepository;
        }
        finally
        {
            if (!resourcesTransferred)
            {
                try
                {
                    if (transaction is not null)
                        await transaction.DisposeAsync();
                }
                finally { await db.DisposeAsync(); }
            }
        }
    }

    internal static IQueryable<MentorshipPair> VisiblePairs(TutorDbContext db, Guid userId) =>
        db.Set<MentorshipPair>().AsNoTracking().Where(p => db.Users.Any(u => u.Id == userId &&
            ((u.Role == UserRole.Mentor && p.Request.TeacherId == userId) ||
             (u.Role == UserRole.Student && p.Request.StudentId == userId))) &&
            db.Users.Any(u => u.Id == p.Request.StudentId) && db.Users.Any(u => u.Id == p.Request.TeacherId));

    internal static IQueryable<LearningGoal> VisibleGoals(TutorDbContext db, Guid userId) =>
        db.Set<LearningGoal>().AsNoTracking().Where(g => VisiblePairs(db, userId).Any(p => p.Id == g.PairId));

    internal static IQueryable<LearningGoal> FilterGoals(TutorDbContext db, Guid userId, GoalListRequest request)
    {
        var goals = VisibleGoals(db, userId);
        if (request.PairId is { } pairId)
            goals = goals.Where(g => g.PairId == pairId);
        return request.Status is { } status ? goals.Where(g => g.Status == status)
            : goals.Where(g => g.Status != GoalStatus.Cancelled);
    }

    internal static IQueryable<GoalDto> GoalPageQuery(IQueryable<LearningGoal> goals, GoalPageRequest request,
        IConfigurationProvider mappings) => goals.OrderByDescending(g => g.CreatedAt).ThenBy(g => g.Id)
        .Skip(Offset(request)).Take(request.PageSize).ProjectTo<GoalDto>(mappings);

    internal static IQueryable<GoalStudentDto> StudentQuery(TutorDbContext db, Guid mentorId) =>
        from pair in VisiblePairs(db, mentorId)
        where pair.Request.TeacherId == mentorId && pair.Status == PairStatus.Active
        join questionnaire in db.Set<Questionnaire>() on pair.Request.StudentId equals questionnaire.UserId into questionnaires
        from questionnaire in questionnaires.DefaultIfEmpty()
        select new GoalStudentDto
        {
            PairId = pair.Id, StudentId = pair.Request.StudentId,
            FirstName = questionnaire == null ? null : questionnaire.FirstName,
            LastName = questionnaire == null ? null : questionnaire.LastName
        };

    internal static IQueryable<SkillProgressCounts> ProgressQuery(TutorDbContext db, Guid userId, Guid? pairId)
    {
        var goals = VisibleGoals(db, userId).Where(g => g.Status != GoalStatus.Cancelled);
        if (pairId.HasValue)
            goals = goals.Where(g => g.PairId == pairId);
        return goals.Select(g => new
            {
                g.SkillId, SkillName = g.Skill.Name, Total = g.Tasks.Count,
                Completed = g.Tasks.Count(t => t.Status == GoalTaskStatus.Completed)
            }).GroupBy(g => new { g.SkillId, g.SkillName })
            .Select(group => new SkillProgressCounts
            {
                SkillId = group.Key.SkillId, SkillName = group.Key.SkillName,
                TotalTasks = group.Sum(g => g.Total), CompletedTasks = group.Sum(g => g.Completed)
            }).OrderBy(s => s.SkillName).ThenBy(s => s.SkillId);
    }

    private static int Offset(GoalPageRequest request) => checked((request.Page - 1) * request.PageSize);

    private static async Task<GoalPage<T>> ReadPageAsync<T>(IQueryable<T> query, GoalPageRequest request, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = Offset(request) >= total ? [] : await query.Skip(Offset(request)).Take(request.PageSize).ToListAsync(ct);
        return new GoalPage<T>(items, total, request.Page, request.PageSize);
    }
}
