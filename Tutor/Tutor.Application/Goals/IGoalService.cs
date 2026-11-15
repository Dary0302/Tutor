using FluentResults;

namespace Tutor.Application.Goals;

public interface IGoalService
{
    Task<GoalPage<GoalDto>> ListAsync(Guid userId, GoalListRequest request, CancellationToken ct);
    Task<Result<GoalDto>> GetAsync(Guid userId, Guid goalId, CancellationToken ct);
    Task<GoalPage<GoalStudentDto>> GetStudentsAsync(Guid mentorId, GoalPageRequest request, CancellationToken ct);
    Task<GoalPage<GoalSkillDto>> GetSkillsAsync(GoalLookupRequest request, CancellationToken ct);
    Task<Result<GoalDto>> CreateAsync(Guid mentorId, CreateGoalRequest request, CancellationToken ct);
    Task<Result<GoalDto>> AddTaskAsync(Guid mentorId, Guid goalId, CreateGoalTaskRequest request, CancellationToken ct);
    Task<Result<GoalDto>> RemoveTaskAsync(Guid mentorId, Guid goalId, Guid taskId, CancellationToken ct);
    Task<Result<GoalDto>> SetTaskCompletionAsync(Guid studentId, Guid goalId, Guid taskId, bool completed, CancellationToken ct);
    Task<Result<GoalDto>> ConfirmAsync(Guid mentorId, Guid goalId, CancellationToken ct);
    Task<Result> CancelAsync(Guid mentorId, Guid goalId, CancellationToken ct);
}
