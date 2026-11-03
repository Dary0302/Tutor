namespace Tutor.Domain.Mentorship;

public sealed class MentorshipRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid StudentId { get; set; }
    public required Guid TeacherId { get; set; }
    public required string StudentLetter { get; set; }
    public RequestStatus Status { get; set; }
    public Guid? CuratorId { get; set; }
}
