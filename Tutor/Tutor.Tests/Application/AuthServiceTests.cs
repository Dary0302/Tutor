using Core.Common.Errors;
using NUnit.Framework;
using Tutor.Application.Auth;
using Tutor.Domain.Users;

namespace Tutor.Tests.Application;

public sealed class AuthServiceTests
{
    private const string Email = "mentor@example.test";
    private const string Password = "test-password";
    private AuthService service = null!;

    [SetUp]
    public void SetUp() => service = new AuthService(new UserRepository(), new TokenIssuer());

    [Test]
    public async Task RegisteredUserCanLogInReadProfileAndRotateRefreshToken()
    {
        Assert.That((await service.RegisterAsync(Email, Password, UserRole.Mentor, default)).IsSuccess, Is.True);
        var login = await service.LoginAsync(Email, Password, default);
        Assert.That(login.IsSuccess, Is.True);
        var session = login.Value;
        Assert.That(session.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(session.Token, Is.EqualTo($"access-{session.Id}"));
        Assert.That(session.RefreshToken, Is.Not.EqualTo(Guid.Empty));
        Assert.That((await service.GetProfileAsync(session.Id, default)).Value, Is.EqualTo(new UserProfile(session.Id, Email, UserRole.Mentor)));

        var refresh = await service.RefreshAsync(session.RefreshToken, default);
        Assert.That(refresh.IsSuccess, Is.True);
        var refreshed = refresh.Value;
        Assert.That(refreshed.Id, Is.EqualTo(session.Id));
        Assert.That(refreshed.Email, Is.EqualTo(Email));
        Assert.That(refreshed.Role, Is.EqualTo(UserRole.Mentor));
        Assert.That(refreshed.Token, Is.EqualTo($"access-{session.Id}"));
        Assert.That(refreshed.RefreshToken, Is.Not.EqualTo(session.RefreshToken));
        Assert.That((await service.RefreshAsync(session.RefreshToken, default)).GetAppError().Status, Is.EqualTo(ErrorStatus.Unauthorized));
        Assert.That((await service.RefreshAsync(refreshed.RefreshToken, default)).IsSuccess, Is.True);
    }

    [Test]
    public async Task DuplicateRegistrationPreservesOriginalCredentialsAndRole()
    {
        await service.RegisterAsync(Email, Password, UserRole.Mentor, default);
        Assert.That((await service.RegisterAsync(Email, "another-password", UserRole.Student, default)).GetAppError().Status, Is.EqualTo(ErrorStatus.Conflict));
        Assert.That((await service.LoginAsync(Email, "another-password", default)).GetAppError().Status, Is.EqualTo(ErrorStatus.Unauthorized));
        var session = await service.LoginAsync(Email, Password, default);
        Assert.That(session.Value.Role, Is.EqualTo(UserRole.Mentor));
    }

    [TestCase(Email, "wrong-password")]
    [TestCase("unknown@example.test", Password)]
    public async Task FailedLoginLeavesExistingSessionUsable(string email, string password)
    {
        await service.RegisterAsync(Email, Password, UserRole.Student, default);
        var session = await service.LoginAsync(Email, Password, default);
        Assert.That((await service.LoginAsync(email, password, default)).GetAppError().Status, Is.EqualTo(ErrorStatus.Unauthorized));
        Assert.That((await service.RefreshAsync(session.Value.RefreshToken, default)).IsSuccess, Is.True);
    }

    [Test]
    public async Task UnknownRefreshTokenAndUserIdReturnNoSessionOrProfile()
    {
        Assert.That((await service.RefreshAsync(Guid.NewGuid(), default)).GetAppError().Status, Is.EqualTo(ErrorStatus.Unauthorized));
        Assert.That((await service.GetProfileAsync(Guid.NewGuid(), default)).GetAppError().Status, Is.EqualTo(ErrorStatus.Unauthorized));
    }

    private sealed class TokenIssuer : IAccessTokenIssuer
    {
        public string Issue(User user) => $"access-{user.Id}";
    }

    private sealed class UserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> users = [];
        private readonly Dictionary<Guid, Guid> sessions = [];

        public Task<bool> TryCreateAsync(User user, CancellationToken ct) =>
            Task.FromResult(users.TryAdd(user.Email, user));

        public Task<User?> FindByEmailAsync(string email, CancellationToken ct) =>
            Task.FromResult(users.GetValueOrDefault(email));

        public Task<User?> FindByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(users.Values.SingleOrDefault(user => user.Id == id));

        public Task<bool> StartSessionAsync(Guid id, Guid newToken, CancellationToken ct)
        {
            if (users.Values.All(user => user.Id != id))
                return Task.FromResult(false);
            sessions[id] = newToken;
            return Task.FromResult(true);
        }

        public Task<User?> RotateSessionAsync(Guid oldToken, Guid newToken, CancellationToken ct)
        {
            var user = users.Values.SingleOrDefault(u => sessions.TryGetValue(u.Id, out var token) && token == oldToken);
            if (user is not null)
                sessions[user.Id] = newToken;
            return Task.FromResult(user);
        }
    }
}
