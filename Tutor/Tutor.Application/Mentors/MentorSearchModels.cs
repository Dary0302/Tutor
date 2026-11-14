using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Mentors;

public sealed class MentorPreferences
{
    public Guid[] SkillIds { get; init; } = [];
    public DayOfWeek[] DaysOfWeek { get; init; } = [];
    public int MinMonths { get; init; }
    public int MaxMonths { get; init; }
    public decimal HoursPerWeek { get; init; }
    public CommunicationFormat CommunicationFormat { get; init; }
}

public sealed class MentorMatch
{
    public required Questionnaire Mentor { get; init; }
    public decimal? Compatibility { get; init; }
}

public sealed class MentorSearchEntry
{
    public required Questionnaire Mentor { get; init; }
    public decimal? Compatibility { get; init; }
    public int AvailablePlaces { get; init; }
}
