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
        return Reply(StatusCodes.Status201Created, "created", newApplication);
    }

    [HttpDelete]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<AppDetailDto>> Delete([FromQuery] Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Reply(StatusCodes.Status204NoContent, "Deleted");
    }

    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<AppListDto>>> Get(CancellationToken cancellationToken)
    {
        var studentId = GetIdAuthorizedUser();

        var result = await service.GetStudentsApplication(studentId, cancellationToken);
        if (result.Count == 0)
        {
            return NotFound();
        }
        return Reply(StatusCodes.Status200OK, "", null, result);
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
        return Reply(StatusCodes.Status200OK, "", null, result);
    }

    [HttpPut("mentor/put")]
    [Authorize(Roles = "Mentor")]
    public async Task<ActionResult> MentorPlaceApprove(PlaceApproveRequest request, CancellationToken cancellationToken)
    {
        await service.MentorPlaceApprove(request.Id, request.StatusApprove, cancellationToken);
        return Reply(StatusCodes.Status200OK, "Approved");
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
        return Reply(StatusCodes.Status200OK, "", null, null, result);
    }

    [HttpPut("tutor/put")]
    [Authorize(Roles = "Tutor")]
    public async Task<ActionResult> TutorPlaceApprove(PlaceApproveRequest request, CancellationToken cancellationToken)
    {
        await service.TutorPlaceApprove(request.Id, request.StatusApprove, cancellationToken);
        return Reply(StatusCodes.Status200OK, "Approved");;
    }
    
    private ObjectResult Reply(int status, string message, AppDetailDto? appDetail = null, List<AppListDto>? listApp = null, List<AppListForTutorDto>? listForTutorApp = null) =>
        StatusCode(status, new AppResponse(message, status, appDetail, listApp, listForTutorApp));
}