namespace Tutor.Domain.Mentorship;

public sealed class MentoringSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required DateTimeOffset StartsAt { get; set; }
    public required DateTimeOffset EndsAt { get; set; }
    public required string Agenda { get; set; }
    public SessionStatus Status { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public SessionCancellationReason? CancellationReason { get; set; }
    public required Guid PairId { get; set; }
}
