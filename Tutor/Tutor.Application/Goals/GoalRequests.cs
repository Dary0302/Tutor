using Tutor.Domain.Mentorship;

namespace Tutor.Application.Goals;

public class GoalPageRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class GoalListRequest : GoalPageRequest
{
    public Guid? PairId { get; init; }
    public GoalStatus? Status { get; init; }
}

public sealed class GoalLookupRequest : GoalPageRequest
{
    public string? Query { get; init; }
}

public sealed class GoalProgressRequest
{
    public Guid? PairId { get; init; }
}

public sealed class CreateGoalRequest
{
    public Guid PairId { get; init; }
    public Guid SkillId { get; init; }
    public required string Title { get; init; }
    public string Description { get; init; } = "";
    public required List<CreateGoalTaskRequest> Tasks { get; init; }
}

public sealed class CreateGoalTaskRequest
{
    public required string Name { get; init; }
}

public sealed class SetGoalTaskCompletionRequest
{
    public required bool IsCompleted { get; init; }
}
