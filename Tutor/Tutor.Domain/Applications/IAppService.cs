using Tutor.Domain.Questionnaires;

namespace Tutor.Domain.Applications;

public interface IAppService
{
    Task<AppDetailDto> CreateApplication(Guid studentId, Guid mentorId, CancellationToken cancellationToken);
    Task<List<AppListDto>> GetMentorsApplication(Guid mentorId, CancellationToken cancellationToken);
    Task MentorPlaceApprove(Guid id, bool statusApprove, CancellationToken cancellationToken);
    Task<List<AppListForTutorDto>> GetAllUnprocessedApps(CancellationToken cancellationToken);
    Task TutorPlaceApprove(Guid id, bool statusApprove, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<AppListDto>> GetStudentsApplication(Guid studentId, CancellationToken cancellationToken);
}