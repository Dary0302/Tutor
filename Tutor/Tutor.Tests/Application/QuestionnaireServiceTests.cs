using Core.Common.Errors;
using NUnit.Framework;
using Tutor.Application.Questionnaires;
using Tutor.Domain.Users;

namespace Tutor.Tests.Application;

public sealed class QuestionnaireServiceTests
{
    private readonly Guid owner = Guid.NewGuid();
    private QuestionnaireService service = null!;

    [SetUp]
    public void SetUp() => service = new QuestionnaireService(new FormRepository(owner));

    [TestCase(UserRole.Student)]
    [TestCase(UserRole.Mentor)]
    public async Task OwnerCanCreateReadAndEditForm(UserRole role)
    {
        var mentor = role == UserRole.Mentor ? MentorInfo() : null;
        var first = Form(mentor);
        Assert.That((await service.GetAsync(owner, default)).GetAppError().Status, Is.EqualTo(ErrorStatus.NotFound));
        Assert.That((await service.SaveAsync(owner, role, first, default)).IsSuccess, Is.True);

        var edit = Form(mentor, skill: "SQL");
        // Adjacent windows and the same hours on another day are valid, regardless of input order.
        edit.Availability.AddRange([Window(DayOfWeek.Monday, 12, 14), Window(DayOfWeek.Tuesday, 10, 12), Window(DayOfWeek.Monday, 10, 12)]);
        Assert.That((await service.SaveAsync(owner, role, edit, default)).IsSuccess, Is.True);
        var saved = (await service.GetAsync(owner, default)).Value;
        Assert.That(saved.Skills.Single().Name, Is.EqualTo("SQL"));
        Assert.That(saved.Availability, Has.Count.EqualTo(3));
        Assert.That(saved.TeacherInfo?.MaxStudents, Is.EqualTo(mentor?.MaxStudents));
        Assert.That((await service.GetAsync(Guid.NewGuid(), default)).GetAppError().Status, Is.EqualTo(ErrorStatus.NotFound));
    }

    [TestCase(UserRole.Mentor)]
    [TestCase(UserRole.Student)]
    public async Task RejectsTeacherInfoThatDoesNotMatchRole(UserRole role)
    {
        var form = Form(role == UserRole.Student ? MentorInfo() : null);
        var result = await service.SaveAsync(owner, role, form, default);
        Assert.That(result.Errors.Single(), Is.TypeOf<FieldValidationError>());
        Assert.That(((FieldValidationError)result.Errors.Single()).Fields.Keys, Is.EquivalentTo(new[] { "teacherInfo" }));
        Assert.That((await service.GetAsync(owner, default)).GetAppError().Status, Is.EqualTo(ErrorStatus.NotFound));
    }

    [Test]
    public async Task InvalidEditReportsAllConflictsAndPreservesSavedForm()
    {
        await service.SaveAsync(owner, UserRole.Student, Form(), default);

        var edit = Form(minMonths: 6, maxMonths: 3, skill: "Аналитика");
        edit.Skills.Add(new FormSkillDto { Name = " АНАЛИТИКА " });
        edit.Availability.AddRange([Window(DayOfWeek.Friday, 14, 16), Window(DayOfWeek.Friday, 13, 15)]);
        var result = await service.SaveAsync(owner, UserRole.Student, edit, default);

        Assert.That(result.Errors.Single(), Is.TypeOf<FieldValidationError>());
        Assert.That(((FieldValidationError)result.Errors.Single()).Fields.Keys, Is.EquivalentTo(new[] { "monthsNumberMax", "skills", "availability" }));
        var saved = (await service.GetAsync(owner, default)).Value;
        Assert.That(saved.Skills.Single().Name, Is.EqualTo("Python"));
        Assert.That(saved.Availability, Is.Empty);
    }

    [Test]
    public async Task TutorCannotCreateForm()
    {
        var result = await service.SaveAsync(owner, UserRole.Tutor, Form(), default);
        Assert.That(result.GetAppError().Status, Is.EqualTo(ErrorStatus.Forbidden));
        Assert.That((await service.GetAsync(owner, default)).GetAppError().Status, Is.EqualTo(ErrorStatus.NotFound));
    }

    [Test]
    public async Task SavingForMissingUserReturnsUnauthorized()
    {
        var result = await service.SaveAsync(Guid.NewGuid(), UserRole.Student, Form(), default);
        Assert.That(result.GetAppError().Status, Is.EqualTo(ErrorStatus.Unauthorized));
    }

    private static QuestionnaireFormDto Form(MentorInfoDto? mentor = null, int minMonths = 6, int maxMonths = 6, string skill = "Python") => new()
    {
        FirstName = "Анна", LastName = "Ли", City = "Пермь", Education = "Университет", GoalDescription = "Изучить ML",
        Skills = [new() { Name = skill }], MonthsNumberMin = minMonths, MonthsNumberMax = maxMonths, TeacherInfo = mentor
    };

    private static MentorInfoDto MentorInfo() => new()
    {
        PlaceOfWork = "Школа", Position = "Преподаватель", YearsExperience = 3, MaxStudents = 2
    };

    private static TimeWindowDto Window(DayOfWeek day, int start, int end) =>
        new() { DayOfWeek = day, StartTime = new(start, 0), EndTime = new(end, 0) };

    private sealed class FormRepository(Guid owner) : IQuestionnaireRepository
    {
        private QuestionnaireFormDto? saved;

        public Task<QuestionnaireFormDto?> GetAsync(Guid userId, CancellationToken ct) =>
            Task.FromResult(userId == owner ? saved : null);

        public Task<bool> SaveAsync(Guid userId, QuestionnaireFormDto form, CancellationToken ct)
        {
            if (userId != owner)
                return Task.FromResult(false);
            saved = form;
            return Task.FromResult(true);
        }
    }
}
