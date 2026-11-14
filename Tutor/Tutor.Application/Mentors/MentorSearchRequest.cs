namespace Tutor.Application.Mentors;

public sealed class MentorSearchRequest
{
    public Guid[] SkillIds { get; init; } = [];
    public int? MinYearsExperience { get; init; }
    public int? MaxYearsExperience { get; init; }
    public int[] DurationMonths { get; init; } = [];
    public DayOfWeek[] DaysOfWeek { get; init; } = [];
    public bool? HasAvailablePlaces { get; init; }
    public decimal? MinHoursPerWeek { get; init; }
    public decimal? MaxHoursPerWeek { get; init; }
    public string? PlaceOfWork { get; init; }
    public decimal? MinRating { get; init; }
    public MentorSort Sort { get; init; } = MentorSort.Compatibility;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public enum MentorSort
{
    Compatibility,
    Rating,
    Experience,
    HoursPerWeek
}
