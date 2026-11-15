using AutoMapper;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Mentors;

public sealed class MentorSearchProfile : Profile
{
    public MentorSearchProfile()
    {
        CreateProjection<Questionnaire, MentorPreferences>()
            .ForMember(x => x.SkillIds, o => o.MapFrom(q => q.Skills.Select(s => s.SkillId)))
            .ForMember(x => x.DaysOfWeek, o => o.MapFrom(q => q.Availability.Select(a => a.DayOfWeek).Distinct()))
            .ForMember(x => x.MinMonths, o => o.MapFrom(q => q.MonthsNumberMin))
            .ForMember(x => x.MaxMonths, o => o.MapFrom(q => q.MonthsNumberMax))
            .ForMember(x => x.HoursPerWeek, o => o.MapFrom(q => q.HoursNumberWeek));
        CreateProjection<QuestionnaireSkill, MentorSkillDto>()
            .ForMember(x => x.Id, o => o.MapFrom(s => s.SkillId))
            .ForMember(x => x.Name, o => o.MapFrom(s => s.Skill.Name));
        CreateProjection<MentorSearchEntry, MentorCardDto>()
            .ForMember(x => x.UserId, o => o.MapFrom(x => x.Mentor.UserId))
            .ForMember(x => x.QuestionnaireId, o => o.MapFrom(x => x.Mentor.Id))
            .ForMember(x => x.FirstName, o => o.MapFrom(x => x.Mentor.FirstName))
            .ForMember(x => x.LastName, o => o.MapFrom(x => x.Mentor.LastName))
            .ForMember(x => x.Position, o => o.MapFrom(x => x.Mentor.TeacherInfo!.Position))
            .ForMember(x => x.PlaceOfWork, o => o.MapFrom(x => x.Mentor.TeacherInfo!.PlaceOfWork))
            .ForMember(x => x.YearsExperience, o => o.MapFrom(x => x.Mentor.TeacherInfo!.YearsExperience))
            .ForMember(x => x.Rating, o => o.MapFrom(x => x.Mentor.TeacherInfo!.Rating))
            .ForMember(x => x.HoursPerWeek, o => o.MapFrom(x => x.Mentor.HoursNumberWeek))
            .ForMember(x => x.Skills, o => o.MapFrom(x => x.Mentor.Skills.OrderBy(s => s.Skill.Name).ThenBy(s => s.SkillId)));
    }
}
