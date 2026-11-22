using Tutor.Domain.Questionnaires;

namespace Tutor.Domain.Applications;

public record AppListForTutorDto(
    Guid Id,
    QuestionnaireFormDto  StudentQuestionnaireForm,
    QuestionnaireFormDto  MentorQuestionnaireForm);