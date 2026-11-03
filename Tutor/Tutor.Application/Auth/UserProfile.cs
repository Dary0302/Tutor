using Tutor.Domain.Users;

namespace Tutor.Application.Auth;

public record UserProfile(Guid Id, string Email, UserRole Role);

public sealed record AuthSession(Guid Id, string Email, UserRole Role, string Token, Guid RefreshToken)
    : UserProfile(Id, Email, Role);
