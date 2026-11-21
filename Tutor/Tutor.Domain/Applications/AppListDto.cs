using Tutor.Domain.Questionnaires;

namespace Tutor.Domain.Applications;

public record AppListDto(
    Guid Id,
    QuestionnaireFormDto  QuestionnaireForm,
    string? reasonForRefusal);