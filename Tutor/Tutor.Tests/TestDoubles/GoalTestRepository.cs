using AutoMapper;
using Tutor.Application.Goals;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;

namespace Tutor.Tests.TestDoubles;

internal sealed class GoalTestRepository(IMapper mapper) : IGoalRepository
{
    public GoalPairContext Pair { get; set; } = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), PairStatus.Active);
    public Skill Skill { get; } = new() { Name = "HTML" };
    public LearningGoal? Goal { get; private set; }
    public int ReadCount { get; private set; }
    public int SaveCount { get; private set; }
    public int DisposeCount { get; private set; }

    public void Reset()
    {
        Pair = Pair with { Status = PairStatus.Active };
        Goal = null;
        ReadCount = SaveCount = DisposeCount = 0;
    }

    public CreateGoalRequest NewGoal(int taskCount = 2) => new()
    {
        PairId = Pair.Id, SkillId = Skill.Id, Title = "  Сверстать дашборд  ",
        Tasks = Enumerable.Range(1, taskCount).Select(i => new CreateGoalTaskRequest { Name = $"  Задача {i}  " }).ToList()
    };

    public Task<GoalPage<GoalDto>> ListAsync(Guid userId, GoalListRequest request, CancellationToken ct)
    {
        ReadCount++;
        var visible = CanSee(userId) && Goal is not null && (!request.PairId.HasValue || request.PairId == Pair.Id) &&
            (request.Status.HasValue ? Goal.Status == request.Status : Goal.Status != GoalStatus.Cancelled);
        return Task.FromResult(new GoalPage<GoalDto>(visible && request.Page == 1 ? [mapper.Map<GoalDto>(Goal)] : [],
            visible ? 1 : 0, request.Page, request.PageSize));
    }

    public Task<GoalDto?> GetAsync(Guid userId, Guid goalId, CancellationToken ct)
    {
        ReadCount++;
        return Task.FromResult(CanSee(userId) && Goal?.Id == goalId && Goal.Status != GoalStatus.Cancelled
            ? mapper.Map<GoalDto>(Goal) : null);
    }

    public Task<GoalPage<GoalStudentDto>> GetStudentsAsync(Guid mentorId, GoalPageRequest request, CancellationToken ct)
    {
        ReadCount++;
        var visible = mentorId == Pair.MentorId && Pair.Status == PairStatus.Active;
        return Task.FromResult(new GoalPage<GoalStudentDto>(visible && request.Page == 1
            ? [new() { PairId = Pair.Id, StudentId = Pair.StudentId, FirstName = "Анна", LastName = "Ли" }] : [],
            visible ? 1 : 0, request.Page, request.PageSize));
    }

    public Task<GoalPage<GoalSkillDto>> GetSkillsAsync(GoalLookupRequest request, CancellationToken ct)
    {
        ReadCount++;
        return Task.FromResult(new GoalPage<GoalSkillDto>([new() { Id = Skill.Id, Name = Skill.Name }], 1, request.Page, request.PageSize));
    }

    public Task<IReadOnlyList<SkillProgressCounts>> GetProgressAsync(Guid userId, Guid? pairId, CancellationToken ct)
    {
        ReadCount++;
        IReadOnlyList<SkillProgressCounts> result = CanSee(userId) && (!pairId.HasValue || pairId == Pair.Id) &&
            Goal is not null && Goal.Status != GoalStatus.Cancelled
            ? [new() { SkillId = Skill.Id, SkillName = Skill.Name, TotalTasks = Goal.Tasks.Count,
                CompletedTasks = Goal.Tasks.Count(t => t.Status == GoalTaskStatus.Completed) }] : [];
        return Task.FromResult(result);
    }

    public Task<IGoalWriteRepository?> EditPairAsync(Guid userId, Guid pairId, CancellationToken ct)
    {
        ReadCount++;
        return Task.FromResult<IGoalWriteRepository?>(CanSee(userId) && pairId == Pair.Id ? new WriteRepository(this) : null);
    }

    public Task<IGoalWriteRepository?> EditGoalAsync(Guid userId, Guid goalId, CancellationToken ct)
    {
        ReadCount++;
        return Task.FromResult<IGoalWriteRepository?>(CanSee(userId) && Goal?.Id == goalId ? new WriteRepository(this) : null);
    }

    private bool CanSee(Guid userId) => userId == Pair.MentorId || userId == Pair.StudentId;

    private sealed class WriteRepository(GoalTestRepository repository) : IGoalWriteRepository
    {
        public GoalPairContext Pair => repository.Pair;
        public LearningGoal? Goal => repository.Goal;
        public Task<Skill?> FindSkillAsync(Guid skillId, CancellationToken ct) =>
            Task.FromResult(skillId == repository.Skill.Id ? repository.Skill : null);
        public void Add(LearningGoal goal) => repository.Goal = goal;
        public Task SaveAsync(CancellationToken ct)
        {
            repository.SaveCount++;
            return Task.CompletedTask;
        }
        public ValueTask DisposeAsync()
        {
            repository.DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }
}
