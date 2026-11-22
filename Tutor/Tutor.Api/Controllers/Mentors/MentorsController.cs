using Core.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Mentors;

namespace Tutor.Controllers.Mentors;

[ApiController]
[Authorize]
[Route("api/mentors")]
public sealed class MentorsController(IMentorSearchService service, IMentorFindService findService) : TutorControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MentorSearchResult>> Search([FromQuery] MentorSearchRequest request, CancellationToken ct)
    {
        var result = await service.SearchAsync(GetIdAuthorizedUser(), request, ct);
        return result.ToActionResult();
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MentorFindResponse>> FindById(Guid id, CancellationToken ct)
    {
        var result = await findService.FindAsync(id, ct);
        return result.ToActionResult();
    }
}
