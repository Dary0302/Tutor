using FluentResults;
using Tutor.Domain.Users;

namespace Tutor.Application.Questionnaires;

public interface IQuestionnaireService
{
    Task<Result<QuestionnaireFormDto>> GetAsync(Guid userId, CancellationToken ct);
    Task<Result> SaveAsync(Guid userId, UserRole role, QuestionnaireFormDto form, CancellationToken ct);
}
