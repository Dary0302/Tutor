using FluentValidation;
using Tutor.Application.Questionnaires;

namespace Tutor.Validators;

public sealed class QuestionnaireFormValidator : AbstractValidator<QuestionnaireFormDto>
{
    public QuestionnaireFormValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.City).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Education).NotEmpty().MaximumLength(255);
        RuleFor(x => x.GoalDescription).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CommunicationFormat).IsInEnum();
        RuleFor(x => x.MonthsNumberMin).GreaterThan(0);
        RuleFor(x => x.MonthsNumberMax).GreaterThan(0);
        RuleFor(x => x.HoursNumberWeek).InclusiveBetween(1, 168);
        RuleFor(x => x.Skills).NotEmpty();
        RuleForEach(x => x.Skills).NotNull().ChildRules(skill =>
        {
            skill.RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
            skill.RuleFor(x => x.SkillLevel).IsInEnum();
        });
        RuleFor(x => x.Availability).NotNull();
        RuleForEach(x => x.Availability).NotNull().ChildRules(slot =>
        {
            slot.RuleFor(x => x.DayOfWeek).IsInEnum();
            slot.RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
        });
        RuleFor(x => x.TeacherInfo).ChildRules(info =>
        {
            info.RuleFor(x => x!.PlaceOfWork).NotEmpty().MaximumLength(255);
            info.RuleFor(x => x!.Position).NotEmpty().MaximumLength(255);
            info.RuleFor(x => x!.YearsExperience).GreaterThanOrEqualTo(0);
            info.RuleFor(x => x!.MaxStudents).GreaterThan(0);
        });
    }
}
