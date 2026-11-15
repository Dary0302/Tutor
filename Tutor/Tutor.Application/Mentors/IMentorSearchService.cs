using FluentResults;

namespace Tutor.Application.Mentors;

public interface IMentorSearchService
{
    Task<Result<MentorSearchResult>> SearchAsync(Guid userId, MentorSearchRequest request, CancellationToken ct);
}
