namespace Tutor.Domain.Questionnaires;

public sealed class Skill
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
}
