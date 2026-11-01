using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IUserRepository
{
    void Create(User entity);
    void Update(User entity);
    void Delete(User entity);
    User GetUserById(Guid id);
    List<User> GetAll();
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    public Task<User?> GetUserByRefreshCode(Guid refreshToken, CancellationToken cancellationToken);
    Task<bool> AnyAsync(string email, CancellationToken cancelationToken);
}