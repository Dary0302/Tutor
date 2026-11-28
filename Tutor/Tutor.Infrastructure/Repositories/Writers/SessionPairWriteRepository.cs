using Microsoft.EntityFrameworkCore.Storage;
using Tutor.Application.Sessions;
using Tutor.Domain.Mentorship;

namespace Tutor.Infrastructure.Repositories.Writers;

internal sealed class SessionPairWriteRepository(TutorDbContext db, IDbContextTransaction transaction,
    SessionPairContext pair) : ISessionPairWriteRepository
{
    public SessionPairContext Pair { get; } = pair;

    public void Add(MentoringSession session) => db.Add(session);

    public async Task SaveAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        try { await transaction.DisposeAsync(); }
        finally { await db.DisposeAsync(); }
    }
}
