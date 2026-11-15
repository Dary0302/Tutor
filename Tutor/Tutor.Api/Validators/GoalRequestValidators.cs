using FluentValidation;
using Tutor.Application.Goals;

namespace Tutor.Validators;

public class GoalPageRequestValidator : AbstractValidator<GoalPageRequest>
{
    public GoalPageRequestValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Page).Cascade(CascadeMode.Stop).GreaterThan(0)
            .Must((request, page) => (page - 1L) * request.PageSize <= int.MaxValue)
            .WithMessage("Номер страницы слишком велик для выбранного размера страницы.");
    }
}

public sealed class GoalListRequestValidator : AbstractValidator<GoalListRequest>
{
    public GoalListRequestValidator()
    {
        Include(new GoalPageRequestValidator());
        RuleFor(x => x.PairId).NotEqual(Guid.Empty);
        RuleFor(x => x.Status).Must(status => !status.HasValue || Enum.IsDefined(status.Value));
    }
}

public sealed class GoalLookupRequestValidator : AbstractValidator<GoalLookupRequest>
{
    public GoalLookupRequestValidator()
    {
        Include(new GoalPageRequestValidator());
        RuleFor(x => x.Query).MaximumLength(255);
    }
}

public sealed class GoalProgressRequestValidator : AbstractValidator<GoalProgressRequest>
{
    public GoalProgressRequestValidator() => RuleFor(x => x.PairId).NotEqual(Guid.Empty);
}

public sealed class CreateGoalRequestValidator : AbstractValidator<CreateGoalRequest>
{
    public CreateGoalRequestValidator()
    {
        RuleFor(x => x.PairId).NotEmpty();
        RuleFor(x => x.SkillId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).NotNull().MaximumLength(255);
        RuleFor(x => x.Tasks).NotEmpty().Must(tasks => tasks is null || tasks.Count <= GoalLimits.MaxTasksPerGoal);
        RuleForEach(x => x.Tasks).NotNull().SetValidator(new CreateGoalTaskRequestValidator());
    }
}

public sealed class CreateGoalTaskRequestValidator : AbstractValidator<CreateGoalTaskRequest>
{
    public CreateGoalTaskRequestValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
}
