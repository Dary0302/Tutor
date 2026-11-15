using AutoMapper;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Goals;

public sealed class GoalProfile : Profile
{
    public GoalProfile()
    {
        CreateMap<CreateGoalRequest, LearningGoal>()
            .ForMember(x => x.Title, o => o.MapFrom(x => x.Title.Trim()))
            .ForMember(x => x.Description, o => o.MapFrom(x => x.Description.Trim()))
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.Status, o => o.Ignore())
            .ForMember(x => x.CreatedAt, o => o.Ignore())
            .ForMember(x => x.ClosedAt, o => o.Ignore())
            .ForMember(x => x.Skill, o => o.Ignore())
            .ForMember(x => x.Pair, o => o.Ignore());
        CreateMap<CreateGoalTaskRequest, GoalTask>()
            .ForMember(x => x.Name, o => o.MapFrom(x => x.Name.Trim()))
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.GoalId, o => o.Ignore())
            .ForMember(x => x.Status, o => o.Ignore())
            .ForMember(x => x.CompletedAt, o => o.Ignore())
            .ForMember(x => x.Position, o => o.Ignore());
        CreateMap<LearningGoal, GoalDto>()
            .ForMember(x => x.SkillName, o => o.MapFrom(x => x.Skill.Name))
            .ForMember(x => x.TotalTasks, o => o.MapFrom(x => x.Tasks.Count))
            .ForMember(x => x.CompletedTasks, o => o.MapFrom(x => x.Tasks.Count(t => t.Status == GoalTaskStatus.Completed)))
            .ForMember(x => x.Tasks, o => o.MapFrom(x => x.Tasks.OrderBy(t => t.Position).ThenBy(t => t.Id)));
        CreateMap<GoalTask, GoalTaskDto>();
        CreateProjection<Skill, GoalSkillDto>();
        CreateMap<SkillProgressCounts, SkillProgressDto>()
            .ForMember(x => x.Percentage, o => o.Ignore());
    }
}
