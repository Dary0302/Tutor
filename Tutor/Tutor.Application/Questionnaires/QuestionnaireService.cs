using Core.Common.Errors;
using FluentResults;
using Tutor.Domain.Users;

namespace Tutor.Application.Questionnaires;

public sealed class QuestionnaireService(IQuestionnaireRepository repository) : IQuestionnaireService
{
    private const string QuestionnaireNotFoundMessage = "Анкета не найдена.";
    private const string MentorInfoRequiredMessage = "Заполните сведения о наставнике.";
    private const string MentorInfoForMentorsOnlyMessage = "Сведения о работе доступны только наставнику.";
    private const string InvalidMonthRangeMessage = "Максимальный срок меньше минимального.";
    private const string DuplicateSkillMessage = "Каждый навык можно указать только один раз.";
    private const string OverlappingTimeWindowsMessage = "Временные окна одного дня пересекаются.";

    public async Task<Result<QuestionnaireFormDto>> GetAsync(Guid userId, CancellationToken ct)
    {
        var form = await repository.GetAsync(userId, ct);
        return form is null ? Result.Fail(AppError.NotFound(QuestionnaireNotFoundMessage)) : Result.Ok(form);
    }

    public async Task<Result> SaveAsync(Guid userId, UserRole role, QuestionnaireFormDto form, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        if (role is not (UserRole.Student or UserRole.Mentor))
            return Result.Fail(AppError.Forbidden());

        if (role == UserRole.Mentor && form.TeacherInfo is null)
            errors["teacherInfo"] = [MentorInfoRequiredMessage];
        if (role == UserRole.Student && form.TeacherInfo is not null)
            errors["teacherInfo"] = [MentorInfoForMentorsOnlyMessage];
        if (form.MonthsNumberMax < form.MonthsNumberMin)
            errors["monthsNumberMax"] = [InvalidMonthRangeMessage];
        if (form.Skills.Select(x => x.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != form.Skills.Count)
            errors["skills"] = [DuplicateSkillMessage];
        foreach (var day in form.Availability.GroupBy(x => x.DayOfWeek))
        {
            var windows = day.OrderBy(x => x.StartTime).ToList();
            if (windows.Zip(windows.Skip(1)).Any(pair => pair.First.EndTime > pair.Second.StartTime))
            {
                errors["availability"] = [OverlappingTimeWindowsMessage];
                break;
            }
        }

        if (errors.Count > 0)
            return Result.Fail(new FieldValidationError(errors));

        var saved = await repository.SaveAsync(userId, form, ct);
        return saved ? Result.Ok() : Result.Fail(AppError.Unauthorized());
    }
}
