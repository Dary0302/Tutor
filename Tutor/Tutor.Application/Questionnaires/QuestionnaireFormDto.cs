using System.Text.Json.Serialization;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Questionnaires;

public sealed class QuestionnaireFormDto
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string City { get; init; }
    public required string Education { get; init; }
    public required string GoalDescription { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter<CommunicationFormat>))]
    public CommunicationFormat CommunicationFormat { get; init; } = CommunicationFormat.Online;
    public int MonthsNumberMin { get; init; } = 6;
    public int MonthsNumberMax { get; init; } = 6;
    public int HoursNumberWeek { get; init; } = 2;
    public required List<FormSkillDto> Skills { get; init; }
    public List<TimeWindowDto> Availability { get; init; } = [];
    public MentorInfoDto? TeacherInfo { get; init; }
}

public sealed class FormSkillDto
{
    public required string Name { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter<SkillLevel>))]
    public SkillLevel SkillLevel { get; init; } = SkillLevel.Beginner;
}

public sealed class TimeWindowDto
{
    [JsonConverter(typeof(JsonStringEnumConverter<DayOfWeek>))]
    public required DayOfWeek DayOfWeek { get; init; }
    public required TimeOnly StartTime { get; init; }
    public required TimeOnly EndTime { get; init; }
}

public sealed class MentorInfoDto
{
    public required string PlaceOfWork { get; init; }
    public required string Position { get; init; }
    public required int YearsExperience { get; init; }
    public required int MaxStudents { get; init; }
}
