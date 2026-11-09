namespace Tutor.Application.Questionnaires;

public interface IQuestionnaireRepository
{
    Task<QuestionnaireFormDto?> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> SaveAsync(Guid userId, QuestionnaireFormDto form, CancellationToken cancellationToken);
}
