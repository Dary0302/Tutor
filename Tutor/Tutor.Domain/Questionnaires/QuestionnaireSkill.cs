namespace Tutor.Domain.Questionnaires;

public sealed class QuestionnaireSkill
{
    public required Guid SkillId { get; init; }
    public required Guid QuestionnaireId { get; init; }
    public SkillLevel SkillLevel { get; set; }
}

public readonly record struct QuestionnaireSkillId(Guid SkillId, Guid QuestionnaireId);
