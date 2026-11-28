using Microsoft.EntityFrameworkCore.Storage;
using Tutor.Application.Sessions;
using Tutor.Domain.Mentorship;

namespace Tutor.Infrastructure.Repositories.Writers;

internal sealed class SessionWriteRepository(TutorDbContext db, IDbContextTransaction transaction,
    MentoringSession session) : ISessionWriteRepository
{
    public MentoringSession Session { get; } = session;

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
