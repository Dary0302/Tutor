using AutoMapper;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Questionnaires;

public sealed class QuestionnaireProfile : Profile
{
    public QuestionnaireProfile()
    {
        ValueTransformers.Add<string>(value => value.Trim());
        CreateMap<QuestionnaireFormDto, Questionnaire>()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.UserId, o => o.Ignore())
            .ForMember(x => x.Skills, o => o.Ignore())
            .ForMember(x => x.Availability, o => o.Ignore())
            .ForMember(x => x.TeacherInfo, o => o.Ignore());
        CreateMap<Questionnaire, QuestionnaireFormDto>();
        CreateMap<QuestionnaireSkill, FormSkillDto>()
            .ForMember(x => x.Name, o => o.MapFrom(x => x.Skill.Name));
        CreateMap<TimeWindowDto, AvailabilitySlot>()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.QuestionnaireId, o => o.Ignore())
            .ReverseMap();
        CreateMap<MentorInfoDto, TeacherInfo>()
            .ForMember(x => x.TeacherQuestionnaireId, o => o.Ignore())
            .ReverseMap();
    }
}
