using System.Linq.Expressions;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Mentors;

public interface IMentorSearchRepository
{
    Task<MentorPreferences?> GetPreferencesAsync(Guid userId, CancellationToken ct);
    Task<MentorSearchResult> SearchAsync(MentorSearchRequest request,
        Expression<Func<Questionnaire, MentorMatch>> compatibility, CancellationToken ct);
}
