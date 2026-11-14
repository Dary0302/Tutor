using FluentValidation;
using Tutor.Application.Mentors;

namespace Tutor.Validators;

public sealed class MentorSearchRequestValidator : AbstractValidator<MentorSearchRequest>
{
    public MentorSearchRequestValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Page).Cascade(CascadeMode.Stop).GreaterThan(0)
            .Must((request, page) => (page - 1L) * request.PageSize <= int.MaxValue)
            .WithMessage("Номер страницы слишком велик для выбранного размера страницы.");
        RuleFor(x => x.Sort).IsInEnum();
        RuleFor(x => x.SkillIds).NotNull().Must(x => x is null || x.Length <= 50);
        RuleForEach(x => x.SkillIds).NotEmpty();
        RuleFor(x => x.DurationMonths).NotNull().Must(x => x is null || x.Length <= 120);
        RuleForEach(x => x.DurationMonths).InclusiveBetween(1, 120);
        RuleFor(x => x.DaysOfWeek).NotNull().Must(x => x is null || x.Length <= 7);
        RuleForEach(x => x.DaysOfWeek).IsInEnum();
        RuleFor(x => x.MinYearsExperience).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxYearsExperience).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxYearsExperience)
            .Must((request, maximum) => !request.MinYearsExperience.HasValue || !maximum.HasValue || maximum >= request.MinYearsExperience)
            .WithMessage("Максимальный опыт меньше минимального.");
        RuleFor(x => x.MinHoursPerWeek).InclusiveBetween(0m, 168m);
        RuleFor(x => x.MaxHoursPerWeek).InclusiveBetween(0m, 168m);
        RuleFor(x => x.MaxHoursPerWeek)
            .Must((request, maximum) => !request.MinHoursPerWeek.HasValue || !maximum.HasValue || maximum >= request.MinHoursPerWeek)
            .WithMessage("Максимальное число часов меньше минимального.");
        RuleFor(x => x.MinRating).InclusiveBetween(0m, 5m);
        RuleFor(x => x.PlaceOfWork).MaximumLength(255);
    }
}
