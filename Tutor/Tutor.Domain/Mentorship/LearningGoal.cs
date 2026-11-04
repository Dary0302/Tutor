namespace Tutor.Domain.Mentorship;

public sealed class LearningGoal
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public GoalStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; set; }
    public required Guid SkillId { get; set; }
    public required Guid PairId { get; set; }
}
