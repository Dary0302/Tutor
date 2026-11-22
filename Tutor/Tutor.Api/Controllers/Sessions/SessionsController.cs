using Core.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Sessions;
using Tutor.Domain.Users;

namespace Tutor.Controllers.Sessions;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student) + "," + nameof(UserRole.Mentor))]
[Route("api/sessions")]
public sealed class SessionsController(ISessionService service) : TutorControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> List([FromQuery] SessionRangeRequest request,
        CancellationToken ct)
    {
        var role = GetRoleAuthorizedUser();
        if (role.IsFailed)
            return role.ToResult<IReadOnlyList<SessionDto>>().ToActionResult();
        return (await service.ListAsync(GetIdAuthorizedUser(), role.Value, request, ct)).ToActionResult();
    }

    [HttpGet("participants")]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public async Task<ActionResult<IReadOnlyList<SessionParticipantDto>>> Participants(CancellationToken ct) =>
        (await service.GetParticipantsAsync(GetIdAuthorizedUser(), ct)).ToActionResult();

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public async Task<ActionResult<SessionDto>> Create(CreateSessionRequest request, CancellationToken ct) =>
        (await service.CreateAsync(GetIdAuthorizedUser(), request, ct)).ToActionResult();

    [HttpPost("{sessionId:guid}/cancel")]
    [Authorize(Roles = nameof(UserRole.Mentor))]
    public async Task<IActionResult> Cancel(Guid sessionId, CancellationToken ct) =>
        (await service.CancelAsync(GetIdAuthorizedUser(), sessionId, ct)).ToActionResult();
}
