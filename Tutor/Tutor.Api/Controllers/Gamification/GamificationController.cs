using Core.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Gamification;
using Tutor.Domain.Users;

namespace Tutor.Controllers.Gamification;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student) + "," + nameof(UserRole.Mentor))]
[Route("api/gamification")]
public sealed class GamificationController(GamificationService service) : TutorControllerBase
{
    [HttpGet("leaderboards/students")]
    public async Task<ActionResult<IReadOnlyList<LeaderboardEntryDto>>> Students([FromQuery] LeaderboardRequest request,
        CancellationToken ct) => (await service.GetLeaderboardAsync(UserRole.Student, request, ct)).ToActionResult();

    [HttpGet("leaderboards/mentors")]
    public async Task<ActionResult<IReadOnlyList<LeaderboardEntryDto>>> Mentors([FromQuery] LeaderboardRequest request,
        CancellationToken ct) => (await service.GetLeaderboardAsync(UserRole.Mentor, request, ct)).ToActionResult();

    [HttpGet("me")]
    public async Task<ActionResult<GamificationProfileDto>> Me(CancellationToken ct)
    {
        var role = GetRoleAuthorizedUser();
        if (role.IsFailed)
            return role.ToResult<GamificationProfileDto>().ToActionResult();
        return (await service.GetProfileAsync(GetIdAuthorizedUser(), role.Value, ct)).ToActionResult();
    }
}
