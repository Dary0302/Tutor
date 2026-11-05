using Core.Common.Errors;
using FluentResults;
using Tutor.Domain.Users;
using BCryptHash = BCrypt.Net.BCrypt;

namespace Tutor.Application.Auth;

public sealed class AuthService(IUserRepository users, IAccessTokenIssuer accessTokens)
{
    private const string EmailAlreadyInUseMessage = "Email already in use";
    private const string InvalidCredentialsMessage = "Invalid email or password";
    private const string InvalidRefreshTokenMessage = "Invalid or expired refresh token";

    public async Task<Result> RegisterAsync(string email, string password, UserRole role, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCryptHash.HashPassword(password),
            Role = role
        };
        var created = await users.TryCreateAsync(user, cancellationToken);
        return created ? Result.Ok() : Result.Fail(AppError.Conflict(EmailAlreadyInUseMessage));
    }

    public async Task<Result<AuthSession>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email, cancellationToken);
        if (user is null || !BCryptHash.Verify(password, user.PasswordHash))
            return Result.Fail(AppError.Unauthorized(InvalidCredentialsMessage));

        var refreshToken = Guid.NewGuid();
        if (!await users.StartSessionAsync(user.Id, refreshToken, cancellationToken))
            return Result.Fail(AppError.Unauthorized(InvalidCredentialsMessage));

        return Result.Ok(CreateSession(user, refreshToken));
    }

    public async Task<Result<AuthSession>> RefreshAsync(Guid refreshToken, CancellationToken cancellationToken)
    {
        var newToken = Guid.NewGuid();
        var user = await users.RotateSessionAsync(refreshToken, newToken, cancellationToken);
        return user is null
            ? Result.Fail(AppError.Unauthorized(InvalidRefreshTokenMessage))
            : Result.Ok(CreateSession(user, newToken));
    }

    public async Task<Result<UserProfile>> GetProfileAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id, cancellationToken);
        return user is null ? Result.Fail(AppError.Unauthorized()) : Result.Ok(new UserProfile(user.Id, user.Email, user.Role));
    }

    private AuthSession CreateSession(User user, Guid refreshToken) =>
        new(user.Id, user.Email, user.Role, accessTokens.Issue(user), refreshToken);
}
