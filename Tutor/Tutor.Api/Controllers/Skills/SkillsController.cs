using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Goals;
using Tutor.Domain.Users;

namespace Tutor.Controllers.Skills;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student) + "," + nameof(UserRole.Mentor))]
[Route("api/skills")]
public class SkillsController(IGoalService service): TutorControllerBase
{
    [HttpGet]
    public Task<GoalPage<GoalSkillDto>> Skills([FromQuery] GoalLookupRequest request, CancellationToken ct) =>
        service.GetSkillsAsync(request, ct);
}