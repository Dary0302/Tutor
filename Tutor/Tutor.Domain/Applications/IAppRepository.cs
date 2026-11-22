namespace Tutor.Domain.Applications;

public interface IAppRepository
{
    Task<bool> TryCreateAsync(App app, CancellationToken cancellationToken);
    Task<List<App>> GetNewAppByMentor(Guid mentorId, CancellationToken cancellationToken);
    Task<List<App>> GetOldAppByMentor(Guid mentorId, CancellationToken cancellationToken);
    Task MentorPlaceApprove(App app, bool statusApprove, string? reasonForRefusal, CancellationToken cancellationToken);
    Task<App?> GetAppById(Guid id, CancellationToken cancellationToken);
    Task<List<App>> GetAllUnprocessedApps(CancellationToken cancellationToken);
    Task TutorPlaceApprove(App app, bool statusApprove, string? reasonForRefusal, CancellationToken cancellationToken);
    Task<bool> TryDeleteAsync(App app, CancellationToken cancellationToken);
    Task<List<App>> GetAppByStudent(Guid studentId, CancellationToken cancellationToken);
}