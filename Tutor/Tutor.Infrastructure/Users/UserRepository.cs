using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure.Users;

internal sealed class UserRepository(TutorDbContext db) : IUserRepository
{
    public async Task<bool> TryCreateAsync(User user, CancellationToken cancellationToken)
    {
        var entry = db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
               { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: TutorDbContext.EmailIndex })
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> StartSessionAsync(Guid id, Guid newToken, CancellationToken cancellationToken)
    {
        var updated = await SetRefreshTokenAsync(db.Users.Where(u => u.Id == id), HashToken(newToken), cancellationToken);
        return updated == 1;
    }

    public async Task<User?> RotateSessionAsync(Guid oldToken, Guid newToken, CancellationToken cancellationToken)
    {
        var oldHash = HashToken(oldToken);
        var newHash = HashToken(newToken);
        var session = db.Users.Where(u => u.RefreshTokenHash == oldHash && u.RefreshTokenExpiresAt > DateTimeOffset.UtcNow);

        // Keep the updated row locked until we have read the owner of the new token.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (await SetRefreshTokenAsync(session, newHash, cancellationToken) == 0)
            return null;

        var user = await db.Users.AsNoTracking().SingleAsync(u => u.RefreshTokenHash == newHash, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return user;
    }

    private static Task<int> SetRefreshTokenAsync(IQueryable<User> users, byte[] hash, CancellationToken cancellationToken) =>
        users.ExecuteUpdateAsync(update => update
            .SetProperty(u => u.RefreshTokenHash, hash)
            .SetProperty(u => u.RefreshTokenExpiresAt, u => DateTimeOffset.UtcNow.AddDays(30))
            .SetProperty(u => u.UpdatedAt, u => DateTimeOffset.UtcNow), cancellationToken);

    // PostgreSQL's uuid_send uses network byte order for the hashes already stored in Users.
    private static byte[] HashToken(Guid token) => SHA256.HashData(token.ToByteArray(bigEndian: true));
}
