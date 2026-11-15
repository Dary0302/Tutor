using System.Text.Json.Serialization;
using Tutor.Domain.Mentorship;

namespace Tutor.Application.Goals;

public sealed record GoalPage<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed class GoalDto
{
    public Guid Id { get; init; }
    public Guid PairId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public Guid SkillId { get; init; }
    public required string SkillName { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter<GoalStatus>))]
    public GoalStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public int TotalTasks { get; init; }
    public int CompletedTasks { get; init; }
    public required List<GoalTaskDto> Tasks { get; init; }
}

public sealed class GoalTaskDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter<GoalTaskStatus>))]
    public GoalTaskStatus Status { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public int Position { get; init; }
}

public sealed class GoalStudentDto
{
    public Guid PairId { get; init; }
    public Guid StudentId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

public sealed class GoalSkillDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}

public sealed class SkillProgressCounts
{
    public Guid SkillId { get; init; }
    public required string SkillName { get; init; }
    public int TotalTasks { get; init; }
    public int CompletedTasks { get; init; }
}

public sealed class SkillProgressDto
{
    public Guid SkillId { get; init; }
    public required string SkillName { get; init; }
    public int TotalTasks { get; init; }
    public int CompletedTasks { get; init; }
    public decimal Percentage { get; set; }
}
