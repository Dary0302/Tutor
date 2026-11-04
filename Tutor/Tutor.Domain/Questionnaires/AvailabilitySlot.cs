namespace Tutor.Domain.Questionnaires;

public sealed class AvailabilitySlot
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid QuestionnaireId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; }
}
