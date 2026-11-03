namespace Tutor.Domain.Mentorship;

public sealed class GoalTask
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public GoalTaskStatus Status { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public required Guid GoalId { get; set; }
}
