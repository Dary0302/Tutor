using Microsoft.EntityFrameworkCore;
using Tutor.Domain.Applications;

namespace Tutor.Infrastructure.Application;

internal sealed class AppRepository(TutorDbContext db): IAppRepository
{

    public async Task<bool> TryCreateAsync(App app, CancellationToken cancellationToken)
    {
        var entry = db.Applications.Add(app);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) 
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }

    public async Task<List<App>> GetNewAppByMentor(Guid mentorId, CancellationToken cancellationToken)
    {
        return await db.Applications.Where(a => a.MentorId == mentorId && a.MentorsAproove == null).ToListAsync(cancellationToken);
    }
    public async Task<List<App>> GetOldAppByMentor(Guid mentorId, CancellationToken cancellationToken)
    {
        return await db.Applications.Where(a => a.MentorId == mentorId && a.MentorsAproove != null).ToListAsync(cancellationToken);
    }

    public async Task MentorPlaceApprove(App app, bool statusApprove, string? reasonForRefusal, CancellationToken cancellationToken)
    {
        app.MentorsAproove =  statusApprove;
        app.ReasonForRefusal = reasonForRefusal;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<App?> GetAppById(Guid id, CancellationToken cancellationToken)
    {
        return await db.Applications.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<App>> GetAllUnprocessedApps(CancellationToken cancellationToken)
    {
        return await db.Applications.Where(a => a.TutorsAproove == null && a.MentorsAproove == true).ToListAsync(cancellationToken);
    }

    public async Task TutorPlaceApprove(App app, bool statusApprove, string? reasonForRefusal, CancellationToken cancellationToken)
    {
        app.TutorsAproove =  statusApprove;
        app.ReasonForRefusal = reasonForRefusal;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TryDeleteAsync(App app, CancellationToken cancellationToken)
    {
        var entry = db.Applications.Remove(app);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) 
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }

    public async Task<List<App>> GetAppByStudent(Guid studentId, CancellationToken cancellationToken)
    {
        return await db.Applications.Where(a => a.StudentId == studentId).ToListAsync(cancellationToken);
    }
}