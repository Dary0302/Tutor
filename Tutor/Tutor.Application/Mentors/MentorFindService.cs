using Core.Common.Errors;
using FluentResults;
using Tutor.Application.Questionnaires;

namespace Tutor.Application.Mentors;

public class MentorFindService(IQuestionnaireRepository questionnaireRepository): IMentorFindService
{
    public async Task<Result<MentorFindResponse>> FindAsync(Guid mentorId, CancellationToken ct)
    {
        var questionnaire = await questionnaireRepository.GetAsync(mentorId, ct);
        if (questionnaire == null) return Result.Fail(AppError.NotFound("Учитель не наёден"));
        var response = new MentorFindResponse
        {
            UserId = mentorId,
            FirstName = questionnaire.FirstName,
            LastName = questionnaire.LastName,
            Position = questionnaire.TeacherInfo.Position,
            PlaceOfWork = questionnaire.TeacherInfo.PlaceOfWork,
            YearsExperience = questionnaire.TeacherInfo.YearsExperience,
            Skills = questionnaire.Skills,
            HoursPerWeek = questionnaire.HoursNumberWeek,
            AvailablePlaces = questionnaire.TeacherInfo.MaxStudents
        };
        return Result.Ok(response);
    }
}