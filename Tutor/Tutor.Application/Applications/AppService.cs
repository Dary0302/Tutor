using Core.Common.Errors;
using FluentResults;
using Tutor.Application.Questionnaires;
using Tutor.Domain.Applications;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Applications;

public class AppService(IAppRepository repository, IQuestionnaireRepository questionnaireRepository): IAppService
{
    public async Task<AppDetailDto> CreateApplication(Guid studentId, Guid mentorId, CancellationToken cancellationToken)
    {
        var newApplication = new App
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            MentorId =  mentorId,
            MentorsAproove = null,
            TutorsAproove =  null
        };

        await repository.TryCreateAsync(newApplication, cancellationToken);
            
        return new AppDetailDto(newApplication.Id, studentId, mentorId);
    }

    public async Task<List<AppListDto>> GetNewMentorsApplication(Guid mentorId, CancellationToken cancellationToken)
    {
        var listApplications = await repository.GetNewAppByMentor(mentorId, cancellationToken);

        List<AppListDto> result = [];

        foreach (var application in listApplications)
        {
            var newQuestionnaire = await questionnaireRepository.GetAsync(application.StudentId, cancellationToken);
            result.Add(new AppListDto(application.Id, newQuestionnaire, application.ReasonForRefusal));
        }
        
        return result;
    }
    
    public async Task<List<AppListDto>> GetOldMentorsApplication(Guid mentorId, CancellationToken cancellationToken)
    {
        var listApplications = await repository.GetOldAppByMentor(mentorId, cancellationToken);

        List<AppListDto> result = [];

        foreach (var application in listApplications)
        {
            var newQuestionnaire = await questionnaireRepository.GetAsync(application.StudentId, cancellationToken);
            result.Add(new AppListDto(application.Id, newQuestionnaire, application.ReasonForRefusal));
        }
        
        return result;
    }

    public async Task MentorPlaceApprove(Guid id, bool statusApprove, string? reasonForRefusal, CancellationToken cancellationToken)
    {
        var app = await repository.GetAppById(id, cancellationToken);
        if (app is null) Result.Fail(AppError.NotFound("Заявка не найдена"));
        
        await repository.MentorPlaceApprove(app, statusApprove, reasonForRefusal, cancellationToken);
    }

    public async Task<List<AppListForTutorDto>> GetAllUnprocessedApps(CancellationToken cancellationToken)
    {
        var listApp = await repository.GetAllUnprocessedApps(cancellationToken);
        if (listApp.Count == 0) Result.Fail(AppError.NotFound("Новых заявок не обнаружено"));

        List<AppListForTutorDto> result = [];
        
        foreach (var app in listApp)
        {
            var studentQuestionnaire = await questionnaireRepository.GetAsync(app.StudentId, cancellationToken);
            var mentorQuestionnaire = await questionnaireRepository.GetAsync(app.MentorId, cancellationToken);
            
            result.Add(new AppListForTutorDto(app.Id, studentQuestionnaire, mentorQuestionnaire));
        }
        
        return result;
    }

    public async Task TutorPlaceApprove(Guid id, bool statusApprove, string? reasonForRefusal, CancellationToken cancellationToken)
    {
        var app = await repository.GetAppById(id, cancellationToken);
        if (app is null) Result.Fail(AppError.NotFound("Заявка не найдена"));
        
        await repository.TutorPlaceApprove(app, statusApprove, reasonForRefusal, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var app = await repository.GetAppById(id, cancellationToken);
        if (app is null) Result.Fail(AppError.NotFound("Заявка не найдена"));
        
        await repository.TryDeleteAsync(app, cancellationToken);
    }

    public async Task<List<AppListDto>> GetStudentsApplication(Guid studentId, CancellationToken cancellationToken)
    {
        var listApplications = await repository.GetAppByStudent(studentId, cancellationToken);

        List<AppListDto> result = [];

        foreach (var application in listApplications)
        {
            var newQuestionnaire = await questionnaireRepository.GetAsync(application.MentorId, cancellationToken);
            result.Add(new AppListDto(application.Id, newQuestionnaire, application.ReasonForRefusal));
        }
        
        return result;
    }
}