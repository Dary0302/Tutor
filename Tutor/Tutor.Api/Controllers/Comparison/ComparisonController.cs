using Core.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Questionnaires;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Users;

namespace Tutor.Controllers.Comparison;

[ApiController]
[Route("api/comparison")]
public class ComparisonController(IQuestionnaireService service) : TutorControllerBase
{
    [HttpGet]
    public async Task<ActionResult<QuestionnaireFormDto>> GetById(
        [FromQuery] Guid id, CancellationToken cancellationToken)
    {
        var form = await service.GetAsync(id, cancellationToken);
        return form.ToActionResult();
    }
}