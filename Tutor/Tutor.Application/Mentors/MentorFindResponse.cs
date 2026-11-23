using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Mentors;

public sealed class MentorFindResponse
{
    public Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Position { get; init; }
    public required string PlaceOfWork { get; init; }
    public int YearsExperience { get; init; }
    public required List<FormSkillDto> Skills { get; init; }
    public decimal HoursPerWeek { get; init; }
    public int AvailablePlaces { get; init; }
}