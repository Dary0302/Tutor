using FluentValidation;
using Tutor.Application.Gamification;

namespace Tutor.Validators;

public sealed class LeaderboardRequestValidator : AbstractValidator<LeaderboardRequest>
{
    public LeaderboardRequestValidator() => RuleFor(x => x.Limit).InclusiveBetween(1, LeaderboardRequest.MaxLimit);
}
