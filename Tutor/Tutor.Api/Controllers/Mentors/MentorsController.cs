using Core.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Mentors;

namespace Tutor.Controllers.Mentors;

[ApiController]
[Authorize]
[Route("api/mentors")]
public sealed class MentorsController(IMentorSearchService service) : TutorControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MentorSearchResult>> Search([FromQuery] MentorSearchRequest request, CancellationToken ct)
    {
        var result = await service.SearchAsync(GetIdAuthorizedUser(), request, ct);
        return result.ToActionResult();
    }
}
