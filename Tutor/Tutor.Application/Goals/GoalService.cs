using AutoMapper;
using Core.Common.Errors;
using FluentResults;
using Tutor.Domain.Mentorship;

namespace Tutor.Application.Goals;

public sealed class GoalService(IGoalRepository repository, IMapper mapper,
    IGoalWorkflowService workflow, TimeProvider timeProvider) : IGoalService
{
    public Task<GoalPage<GoalDto>> ListAsync(Guid userId, GoalListRequest request, CancellationToken ct) =>
        repository.ListAsync(userId, request, ct);

    public async Task<Result<GoalDto>> GetAsync(Guid userId, Guid goalId, CancellationToken ct)
    {
        var goal = await repository.GetAsync(userId, goalId, ct);
        return goal is null ? Result.Fail(AppError.NotFound("Цель не найдена.")) : Result.Ok(goal);
    }

    public Task<GoalPage<GoalStudentDto>> GetStudentsAsync(Guid mentorId, GoalPageRequest request, CancellationToken ct) =>
        repository.GetStudentsAsync(mentorId, request, ct);

    public Task<GoalPage<GoalSkillDto>> GetSkillsAsync(GoalLookupRequest request, CancellationToken ct) =>
        repository.GetSkillsAsync(request, ct);

    public async Task<Result<GoalDto>> CreateAsync(Guid mentorId, CreateGoalRequest request, CancellationToken ct)
    {
        await using var edit = await repository.EditPairAsync(mentorId, request.PairId, ct);
        if (edit is null)
            return Result.Fail(AppError.NotFound("Пара наставничества не найдена."));
        var access = CanChange(edit.Pair, mentorId, asMentor: true);
        if (access.IsFailed)
            return access.ToResult<GoalDto>();
        var skill = await edit.FindSkillAsync(request.SkillId, ct);
        if (skill is null)
            return Result.Fail(new FieldValidationError(new() { ["skillId"] = ["Навык не найден."] }));

        var goal = mapper.Map<LearningGoal>(request);
        goal.Skill = skill;
        goal.CreatedAt = timeProvider.GetUtcNow();
        for (var i = 0; i < goal.Tasks.Count; i++)
        {
            goal.Tasks[i].GoalId = goal.Id;
            goal.Tasks[i].Position = i;
        }
        edit.Add(goal);
        await edit.SaveAsync(ct);
        return Result.Ok(mapper.Map<GoalDto>(goal));
    }

    public Task<Result<GoalDto>> AddTaskAsync(Guid mentorId, Guid goalId, CreateGoalTaskRequest request, CancellationToken ct) =>
        ChangeAsync(mentorId, goalId, true, goal => workflow.AddTask(goal, mapper.Map<GoalTask>(request)), ct);

    public Task<Result<GoalDto>> RemoveTaskAsync(Guid mentorId, Guid goalId, Guid taskId, CancellationToken ct) =>
        ChangeAsync(mentorId, goalId, true, goal => workflow.RemoveTask(goal, taskId), ct);

    public Task<Result<GoalDto>> SetTaskCompletionAsync(Guid studentId, Guid goalId, Guid taskId, bool completed, CancellationToken ct) =>
        ChangeAsync(studentId, goalId, false, goal => workflow.SetTaskCompletion(goal, taskId, completed), ct);

    public Task<Result<GoalDto>> ConfirmAsync(Guid mentorId, Guid goalId, CancellationToken ct) =>
        ChangeAsync(mentorId, goalId, true, workflow.Confirm, ct);

    public async Task<Result> CancelAsync(Guid mentorId, Guid goalId, CancellationToken ct) =>
        (await ChangeAsync(mentorId, goalId, true, workflow.Cancel, ct)).ToResult();

    private async Task<Result<GoalDto>> ChangeAsync(Guid userId, Guid goalId, bool asMentor,
        Func<LearningGoal, Result> change, CancellationToken ct)
    {
        await using var edit = await repository.EditGoalAsync(userId, goalId, ct);
        if (edit?.Goal is null)
            return Result.Fail(AppError.NotFound("Цель не найдена."));
        var access = CanChange(edit.Pair, userId, asMentor);
        if (access.IsFailed)
            return access.ToResult<GoalDto>();
        var result = change(edit.Goal);
        if (result.IsFailed)
            return result.ToResult<GoalDto>();

        await edit.SaveAsync(ct);
        return Result.Ok(mapper.Map<GoalDto>(edit.Goal));
    }

    private static Result CanChange(GoalPairContext pair, Guid userId, bool asMentor)
    {
        if (userId != (asMentor ? pair.MentorId : pair.StudentId))
            return Result.Fail(AppError.Forbidden());
        return pair.Status == PairStatus.Active ? Result.Ok()
            : Result.Fail(AppError.Conflict("Изменения доступны только в активной паре наставничества."));
    }
}
