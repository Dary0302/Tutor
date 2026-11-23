using FluentResults;

namespace Tutor.Application.Mentors;

public interface IMentorFindService
{
    Task<Result<MentorFindResponse>> FindAsync(Guid mentorId, CancellationToken ct);
}