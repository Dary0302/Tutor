using FluentResults;
using Tutor.Domain.Users;

namespace Tutor.Application.Sessions;

public interface ISessionService
{
    Task<Result<IReadOnlyList<SessionDto>>> ListAsync(Guid userId, UserRole role,
        SessionRangeRequest request, CancellationToken ct);
    Task<Result<IReadOnlyList<SessionParticipantDto>>> GetParticipantsAsync(Guid mentorId, CancellationToken ct);
    Task<Result<SessionDto>> CreateAsync(Guid mentorId, CreateSessionRequest request, CancellationToken ct);
    Task<Result> CancelAsync(Guid mentorId, Guid sessionId, CancellationToken ct);
}
