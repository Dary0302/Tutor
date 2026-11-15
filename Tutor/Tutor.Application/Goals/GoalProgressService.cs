using AutoMapper;

namespace Tutor.Application.Goals;

public sealed class GoalProgressService(IGoalRepository repository, IMapper mapper) : IGoalProgressService
{
    public async Task<IReadOnlyList<SkillProgressDto>> GetAsync(Guid userId, Guid? pairId, CancellationToken ct)
    {
        var counts = await repository.GetProgressAsync(userId, pairId, ct);
        var progress = mapper.Map<List<SkillProgressDto>>(counts);
        foreach (var skill in progress)
            skill.Percentage = skill.TotalTasks == 0 ? 0
                : Math.Round(100m * skill.CompletedTasks / skill.TotalTasks, 0, MidpointRounding.AwayFromZero);
        return progress;
    }
}
