using FluentValidation;

namespace Auth.Application.UseCases.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Role).NotEmpty().NotNull();
        RuleFor(x => x.Email).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(3).MaximumLength(100);
    }
}