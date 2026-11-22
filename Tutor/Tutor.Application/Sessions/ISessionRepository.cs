using Tutor.Domain.Mentorship;
using Tutor.Domain.Users;

namespace Tutor.Application.Sessions;

public interface ISessionRepository
{
    Task<IReadOnlyList<SessionDto>> ListAsync(Guid userId, UserRole role, DateTimeOffset fromUtc,
        DateTimeOffset toUtc, CancellationToken ct);
    Task<IReadOnlyList<SessionParticipantDto>> GetParticipantsAsync(Guid mentorId, CancellationToken ct);
    Task<ISessionPairWriteRepository?> EditPairAsync(Guid mentorId, Guid pairId, CancellationToken ct);
    Task<ISessionWriteRepository?> EditSessionAsync(Guid mentorId, Guid sessionId, CancellationToken ct);
}

public interface ISessionPairWriteRepository : IAsyncDisposable
{
    SessionPairContext Pair { get; }
    void Add(MentoringSession session);
    Task SaveAsync(CancellationToken ct);
}

public interface ISessionWriteRepository : IAsyncDisposable
{
    MentoringSession Session { get; }
    Task SaveAsync(CancellationToken ct);
}

public sealed record SessionPairContext(Guid Id, Guid MentorId, Guid StudentId, PairStatus Status,
    string? StudentName);
