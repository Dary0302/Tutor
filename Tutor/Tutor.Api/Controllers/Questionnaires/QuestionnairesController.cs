using Core.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Questionnaires;
using Tutor.Domain.Users;

namespace Tutor.Controllers.Questionnaires;

[ApiController]
[Authorize(Roles = nameof(UserRole.Student) + "," + nameof(UserRole.Mentor))]
[Route("api/questionnaires/me")]
public sealed class QuestionnairesController(IQuestionnaireService service) : TutorControllerBase
{
    [HttpGet]
    public async Task<ActionResult<QuestionnaireFormDto>> Get(CancellationToken ct)
    {
        var form = await service.GetAsync(GetIdAuthorizedUser(), ct);
        return form.ToActionResult();
    }

    [HttpPut]
    public async Task<IActionResult> Save(QuestionnaireFormDto form, CancellationToken ct)
    {
        var roleResult = GetRoleAuthorizedUser();
        if (roleResult.IsFailed)
            return roleResult.ToResult().ToActionResult();

        var result = await service.SaveAsync(GetIdAuthorizedUser(), roleResult.Value, form, ct);
        return result.ToActionResult();
    }
}
