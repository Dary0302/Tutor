namespace Tutor.Domain.Applications;

public sealed class App
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public Guid MentorId { get; init; }
    public bool? MentorsAproove { get; set; }
    public bool? TutorsAproove { get; set; }
}