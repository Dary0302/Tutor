using Core.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Goals;
using Tutor.Domain.Users;

namespace Tutor.Controllers.Goals;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student) + "," + nameof(UserRole.Mentor))]
[Route("api/goals")]
public sealed class GoalsController(IGoalService service, IGoalProgressService progressService) : TutorControllerBase
{
    [HttpGet]
    public Task<GoalPage<GoalDto>> List([FromQuery] GoalListRequest request, CancellationToken ct) =>
        service.ListAsync(GetIdAuthorizedUser(), request, ct);

    [HttpGet("{goalId:guid}")]
    public async Task<ActionResult<GoalDto>> Get(Guid goalId, CancellationToken ct) =>
        (await service.GetAsync(GetIdAuthorizedUser(), goalId, ct)).ToActionResult();

    [HttpGet("students")]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public Task<GoalPage<GoalStudentDto>> Students([FromQuery] GoalPageRequest request, CancellationToken ct) =>
        service.GetStudentsAsync(GetIdAuthorizedUser(), request, ct);

    [HttpGet("skills")]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public Task<GoalPage<GoalSkillDto>> Skills([FromQuery] GoalLookupRequest request, CancellationToken ct) =>
        service.GetSkillsAsync(request, ct);

    [HttpGet("progress")]
    public Task<IReadOnlyList<SkillProgressDto>> Progress([FromQuery] GoalProgressRequest request, CancellationToken ct) =>
        progressService.GetAsync(GetIdAuthorizedUser(), request.PairId, ct);

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public async Task<ActionResult<GoalDto>> Create(CreateGoalRequest request, CancellationToken ct) =>
        (await service.CreateAsync(GetIdAuthorizedUser(), request, ct)).ToActionResult();

    [HttpPost("{goalId:guid}/tasks")]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public async Task<ActionResult<GoalDto>> AddTask(Guid goalId, CreateGoalTaskRequest request, CancellationToken ct) =>
        (await service.AddTaskAsync(GetIdAuthorizedUser(), goalId, request, ct)).ToActionResult();

    [HttpDelete("{goalId:guid}/tasks/{taskId:guid}")]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public async Task<ActionResult<GoalDto>> RemoveTask(Guid goalId, Guid taskId, CancellationToken ct) =>
        (await service.RemoveTaskAsync(GetIdAuthorizedUser(), goalId, taskId, ct)).ToActionResult();

    [HttpPut("{goalId:guid}/tasks/{taskId:guid}/completion")]
    [Authorize(Roles = nameof(UserRole.Student))]
    public async Task<ActionResult<GoalDto>> SetTaskCompletion(Guid goalId, Guid taskId,
        SetGoalTaskCompletionRequest request, CancellationToken ct) =>
        (await service.SetTaskCompletionAsync(GetIdAuthorizedUser(), goalId, taskId, request.IsCompleted, ct)).ToActionResult();

    [HttpPost("{goalId:guid}/confirm")]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public async Task<ActionResult<GoalDto>> Confirm(Guid goalId, CancellationToken ct) =>
        (await service.ConfirmAsync(GetIdAuthorizedUser(), goalId, ct)).ToActionResult();

    [HttpDelete("{goalId:guid}")]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public async Task<IActionResult> Cancel(Guid goalId, CancellationToken ct) =>
        (await service.CancelAsync(GetIdAuthorizedUser(), goalId, ct)).ToActionResult();
}
