using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Domain.Applications;
using Tutor.Domain.Questionnaires;

namespace Tutor.Controllers.Applications;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController(IAppService service): TutorControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<AppDetailDto>> Create(
        [FromBody] Guid mentorId, CancellationToken cancellationToken)
    {
        var studentId = GetIdAuthorizedUser();

        var newApplication = await service.CreateApplication(studentId, mentorId, cancellationToken);
        return newApplication;
    }

    [HttpGet("mentor/get")]
    [Authorize(Roles = "Mentor")]
    public async Task<ActionResult<List<AppListDto>>> GetMentorApplications(CancellationToken cancellationToken)
    {
        var mentorId = GetIdAuthorizedUser();

        var result = await service.GetMentorsApplication(mentorId, cancellationToken);
        if (result.Count == 0)
        {
            return NotFound();
        }
        return result;
    }

    [HttpPut("mentor/put")]
    [Authorize(Roles = "Mentor")]
    public async Task<ActionResult<AppDetailDto>> MentorPlaceApprove(PlaceApproveRequest request, CancellationToken cancellationToken)
    {
        await service.MentorPlaceApprove(request.Id, request.StatusApprove, cancellationToken);
        return new AppDetailDto(new Guid(), new Guid(), new Guid());
    }
    
    [HttpGet("tutor/get")]
    [Authorize(Roles = "Tutor")]
    public async Task<ActionResult<List<AppListForTutorDto>>> GetTutorApplications(CancellationToken cancellationToken)
    {
        var result = await service.GetAllUnprocessedApps(cancellationToken);
        if (result.Count == 0)
        {
            return NotFound();
        }
        return result;
    }

    [HttpPut("tutor/put")]
    [Authorize(Roles = "Tutor")]
    public async Task<ActionResult<AppDetailDto>> TutorPlaceApprove(PlaceApproveRequest request, CancellationToken cancellationToken)
    {
        await service.TutorPlaceApprove(request.Id, request.StatusApprove, cancellationToken);
        return new AppDetailDto(new Guid(), new Guid(), new Guid());
    }
}