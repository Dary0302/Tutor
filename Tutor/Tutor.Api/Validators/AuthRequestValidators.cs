using System.Text;
using FluentValidation;
using Tutor.Controllers.Users;

namespace Tutor.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().Length(3, 255);
        RuleFor(request => request.Password).NotEmpty().Length(3, 100);
    }
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private const string PasswordTooLongMessage = "Password must fit within the 72-byte bcrypt limit.";

    public RegisterRequestValidator()
    {
        Include(new LoginRequestValidator());
        RuleFor(request => request.Email).EmailAddress();
        RuleFor(request => request.Role).IsInEnum();
        RuleFor(request => request.Password)
            .Must(password => password is null || Encoding.UTF8.GetByteCount(password) <= 72)
            .WithMessage(PasswordTooLongMessage);
    }
}

public sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(request => request.RefreshToken).NotEmpty();
    }
}
