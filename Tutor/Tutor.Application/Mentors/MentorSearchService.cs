using FluentResults;

namespace Tutor.Application.Mentors;

public sealed class MentorSearchService(
    IMentorSearchRepository mentorSearchRepository,
    IMentorCompatibilityService compatibilityService) : IMentorSearchService
{
    public async Task<Result<MentorSearchResult>> SearchAsync(Guid userId, MentorSearchRequest request, CancellationToken ct)
    {
        var preferences = await mentorSearchRepository.GetPreferencesAsync(userId, ct);
        var projection = compatibilityService.CreateProjection(preferences);
        return Result.Ok(await mentorSearchRepository.SearchAsync(request, projection, ct));
    }
}
