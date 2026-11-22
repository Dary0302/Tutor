using FluentValidation;
using Tutor.Application.Sessions;

namespace Tutor.Validators;

public sealed class SessionRangeRequestValidator : AbstractValidator<SessionRangeRequest>
{
    public SessionRangeRequestValidator()
    {
        RuleFor(x => x.From).NotEqual(default(DateTimeOffset));
        RuleFor(x => x.To).NotEqual(default(DateTimeOffset))
            .GreaterThan(x => x.From)
            .WithMessage("Конец диапазона должен быть позже его начала.");
    }
}

public sealed class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.PairId).NotEmpty();
        RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt)
            .WithMessage("Время окончания должно быть позже времени начала.");
        RuleFor(x => x.Agenda).NotEmpty().MaximumLength(255);
    }
}
