using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class UserRepository: IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public void Create(User entity)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        _context.Add(entity);
    }

    public void Update(User entity)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Add(entity);
    }

    public void Delete(User entity)
    {
        entity.DeletedAt = DateTimeOffset.UtcNow;
        _context.Add(entity);
    }

    public User? GetUserById(Guid id)
    {
        return _context.Set<User>().FirstOrDefault(x => x.Id == id);
    }

    public List<User> GetAll()
    {
        return _context.Set<User>().ToList();
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _context.Users
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetUserByRefreshCode(Guid refreshToken, CancellationToken cancellationToken)
    {
        return _context.Users
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken: cancellationToken);
    }

    public Task<bool> AnyAsync(string email, CancellationToken cancelationToken)
    {
        return _context.Users
            .AnyAsync(x => x.Email == email, cancelationToken);
    }
}