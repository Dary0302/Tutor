namespace Tutor.Domain.Mentorship;

public sealed class MentorshipPair
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public PairStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndedAt { get; set; }
    public PairEndReason? EndReason { get; set; }
    public Guid? EndedByUserId { get; set; }
    public required Guid RequestId { get; set; }
}
