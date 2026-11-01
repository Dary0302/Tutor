using FluentValidation;

namespace Auth.Application.UseCases.Authentication;

public class AuthenticationValidator : AbstractValidator<AuthenticationRequest>
{
    public AuthenticationValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(3).MaximumLength(100);
    }
}