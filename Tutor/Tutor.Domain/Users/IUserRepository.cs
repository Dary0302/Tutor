namespace Tutor.Domain.Users;

public interface IUserRepository
{
    Task<bool> TryCreateAsync(User user, CancellationToken cancellationToken);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> StartSessionAsync(Guid id, Guid newToken, CancellationToken cancellationToken);
    Task<User?> RotateSessionAsync(Guid oldToken, Guid newToken, CancellationToken cancellationToken);
}
