using Tutor.Domain.Users;
using BCryptHash = BCrypt.Net.BCrypt;

namespace Tutor.Application.Auth;

public sealed class AuthService(IUserRepository users, IAccessTokenIssuer accessTokens)
{
    public Task<bool> RegisterAsync(string email, string password, UserRole role, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCryptHash.HashPassword(password),
            Role = role
        };
        return users.TryCreateAsync(user, cancellationToken);
    }

    public async Task<AuthSession?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email, cancellationToken);
        if (user is null || !BCryptHash.Verify(password, user.PasswordHash))
            return null;

        var refreshToken = Guid.NewGuid();
        if (!await users.StartSessionAsync(user.Id, refreshToken, cancellationToken))
            return null;

        return CreateSession(user, refreshToken);
    }

    public async Task<AuthSession?> RefreshAsync(Guid refreshToken, CancellationToken cancellationToken)
    {
        var newToken = Guid.NewGuid();
        var user = await users.RotateSessionAsync(refreshToken, newToken, cancellationToken);
        return user is null ? null : CreateSession(user, newToken);
    }

    public async Task<UserProfile?> GetProfileAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id, cancellationToken);
        return user is null ? null : new UserProfile(user.Id, user.Email, user.Role);
    }

    private AuthSession CreateSession(User user, Guid refreshToken) =>
        new(user.Id, user.Email, user.Role, accessTokens.Issue(user), refreshToken);
}
