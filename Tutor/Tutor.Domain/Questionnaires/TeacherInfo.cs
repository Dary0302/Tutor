namespace Tutor.Domain.Questionnaires;

public sealed class TeacherInfo
{
    public required Guid TeacherQuestionnaireId { get; init; }
    public required string PlaceOfWork { get; set; }
    public required string Position { get; set; }
    public int YearsExperience { get; set; }
    public int MaxStudents { get; set; }
}
