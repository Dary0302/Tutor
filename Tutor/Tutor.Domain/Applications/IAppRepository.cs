namespace Tutor.Domain.Applications;

public interface IAppRepository
{
    Task<bool> TryCreateAsync(App app, CancellationToken cancellationToken);
    Task<List<App>> GetAppByMentor(Guid mentorId, CancellationToken cancellationToken);
    Task MentorPlaceApprove(App app, bool statusApprove, CancellationToken cancellationToken);
    Task<App?> GetAppById(Guid id, CancellationToken cancellationToken);
    Task<List<App>> GetAllUnprocessedApps(CancellationToken cancellationToken);
    Task TutorPlaceApprove(App app, bool statusApprove, CancellationToken cancellationToken);
}