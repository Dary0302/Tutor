namespace Tutor.Application.Mentors;

public sealed record MentorSearchResult(IReadOnlyList<MentorCardDto> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed class MentorCardDto
{
    public Guid UserId { get; init; }
    public Guid QuestionnaireId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Position { get; init; }
    public required string PlaceOfWork { get; init; }
    public int YearsExperience { get; init; }
    public required List<MentorSkillDto> Skills { get; init; }
    public decimal? Rating { get; init; }
    public decimal HoursPerWeek { get; init; }
    public int AvailablePlaces { get; init; }
    public decimal? Compatibility { get; init; }
}

public sealed class MentorSkillDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}
