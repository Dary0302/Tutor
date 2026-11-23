using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tutor.Application.Sessions;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Users;

namespace Tutor.Infrastructure.Repositories;

internal sealed class SessionRepository(IDbContextFactory<TutorDbContext> factory) : ISessionRepository
{
    public async Task<IReadOnlyList<SessionDto>> ListAsync(Guid userId, UserRole role, DateTimeOffset fromUtc,
        DateTimeOffset toUtc, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var pairs = VisiblePairs(db, userId, role);
        var query = from session in db.Set<MentoringSession>().AsNoTracking()
                    join pair in pairs on session.PairId equals pair.Id
                    where session.StartsAt >= fromUtc && session.StartsAt < toUtc
                    orderby session.StartsAt, session.Id
                    select new SessionDto(session.Id, session.PairId, session.StartsAt, session.EndsAt,
                        session.Agenda, session.Status,
                        db.Set<Questionnaire>().Where(q => q.UserId ==
                                (role == UserRole.Mentor ? pair.Request.StudentId : pair.Request.TeacherId))
                            .Select(q => q.FirstName + " " + q.LastName).SingleOrDefault());
        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SessionParticipantDto>> GetParticipantsAsync(Guid mentorId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var pairs = VisiblePairs(db, mentorId, UserRole.Mentor).Where(p => p.Status == PairStatus.Active);
        var query = from pair in pairs
                    join questionnaire in db.Set<Questionnaire>().AsNoTracking()
                        on pair.Request.StudentId equals questionnaire.UserId into questionnaires
                    from questionnaire in questionnaires.DefaultIfEmpty()
                    orderby questionnaire == null ? "" : questionnaire.LastName,
                        questionnaire == null ? "" : questionnaire.FirstName, pair.Id
                    select new SessionParticipantDto(pair.Id, pair.Request.StudentId,
                        questionnaire == null ? null : questionnaire.FirstName,
                        questionnaire == null ? null : questionnaire.LastName);
        return await query.ToListAsync(ct);
    }

    public Task<ISessionPairWriteRepository?> EditPairAsync(Guid mentorId, Guid pairId, CancellationToken ct) =>
        OpenPairEditAsync(mentorId, pairId, ct);

    public Task<ISessionWriteRepository?> EditSessionAsync(Guid mentorId, Guid sessionId, CancellationToken ct) =>
        OpenSessionEditAsync(mentorId, sessionId, ct);

    private async Task<ISessionPairWriteRepository?> OpenPairEditAsync(Guid mentorId, Guid pairId, CancellationToken ct)
    {
        var db = await factory.CreateDbContextAsync(ct);
        IDbContextTransaction? transaction = null;
        var resourcesTransferred = false;
        try
        {
            var visiblePairId = await VisiblePairs(db, mentorId, UserRole.Mentor)
                .Where(p => p.Id == pairId).Select(p => (Guid?)p.Id).SingleOrDefaultAsync(ct);
            if (!visiblePairId.HasValue)
                return null;

            transaction = await db.Database.BeginTransactionAsync(ct);
            var locked = await db.Set<MentorshipPair>()
                .FromSql($"SELECT * FROM pairs WHERE id = {visiblePairId.Value} FOR UPDATE")
                .SingleOrDefaultAsync(ct);
            if (locked is null)
                return null;

            var pair = await VisiblePairs(db, mentorId, UserRole.Mentor).AsTracking().Include(p => p.Request)
                .SingleOrDefaultAsync(p => p.Id == visiblePairId.Value, ct);
            if (pair is null)
                return null;

            var studentName = await db.Set<Questionnaire>().AsNoTracking()
                .Where(q => q.UserId == pair.Request.StudentId)
                .Select(q => q.FirstName + " " + q.LastName).SingleOrDefaultAsync(ct);
            var repository = new SessionPairWriteRepository(db, transaction,
                new SessionPairContext(pair.Id, pair.Request.TeacherId, pair.Request.StudentId, pair.Status, studentName));
            resourcesTransferred = true;
            return repository;
        }
        finally
        {
            if (!resourcesTransferred)
            {
                try
                {
                    if (transaction is not null)
                        await transaction.DisposeAsync();
                }
                finally { await db.DisposeAsync(); }
            }
        }
    }

    private async Task<ISessionWriteRepository?> OpenSessionEditAsync(Guid mentorId, Guid sessionId, CancellationToken ct)
    {
        var db = await factory.CreateDbContextAsync(ct);
        IDbContextTransaction? transaction = null;
        var resourcesTransferred = false;
        try
        {
            var visibleSessionId = await (from item in db.Set<MentoringSession>().AsNoTracking()
                                          join pair in VisiblePairs(db, mentorId, UserRole.Mentor)
                                              on item.PairId equals pair.Id
                                          where item.Id == sessionId
                                          select (Guid?)item.Id).SingleOrDefaultAsync(ct);
            if (!visibleSessionId.HasValue)
                return null;

            transaction = await db.Database.BeginTransactionAsync(ct);
            var locked = await db.Set<MentoringSession>()
                .FromSql($"SELECT * FROM sessions WHERE id = {visibleSessionId.Value} FOR UPDATE")
                .SingleOrDefaultAsync(ct);
            if (locked is null)
                return null;

            var session = await (from item in db.Set<MentoringSession>().AsTracking()
                                 join pair in VisiblePairs(db, mentorId, UserRole.Mentor)
                                     on item.PairId equals pair.Id
                                 where item.Id == visibleSessionId.Value
                                 select item).SingleOrDefaultAsync(ct);
            if (session is null)
                return null;

            var repository = new SessionWriteRepository(db, transaction, session);
            resourcesTransferred = true;
            return repository;
        }
        finally
        {
            if (!resourcesTransferred)
            {
                try
                {
                    if (transaction is not null)
                        await transaction.DisposeAsync();
                }
                finally { await db.DisposeAsync(); }
            }
        }
    }

    private static IQueryable<MentorshipPair> VisiblePairs(TutorDbContext db, Guid userId, UserRole role) =>
        db.Set<MentorshipPair>().AsNoTracking().Where(pair =>
            db.Users.Any(user => user.Id == userId && user.Role == role) &&
            (role == UserRole.Mentor ? pair.Request.TeacherId == userId : pair.Request.StudentId == userId) &&
            db.Users.Any(user => user.Id == pair.Request.StudentId) &&
            db.Users.Any(user => user.Id == pair.Request.TeacherId));

    private sealed class SessionPairWriteRepository(TutorDbContext db, IDbContextTransaction transaction,
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

    private sealed class SessionWriteRepository(TutorDbContext db, IDbContextTransaction transaction,
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
}
