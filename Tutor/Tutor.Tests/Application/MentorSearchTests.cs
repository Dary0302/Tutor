using System.Linq.Expressions;
using NUnit.Framework;
using Tutor.Application.Mentors;
using Tutor.Domain.Questionnaires;

namespace Tutor.Tests.Application;

public sealed class MentorSearchTests
{
    [TestCase(10, 3)]
    [TestCase(7, 3)]
    [TestCase(20, 2)]
    public async Task UserWithoutQuestionnaireCanSearchAndKeepsRequestedPage(int pageSize, int totalPages)
    {
        var repository = new SearchRepository();
        var service = CreateService(repository);
        var result = await service.SearchAsync(Guid.NewGuid(), new MentorSearchRequest { Page = 2, PageSize = pageSize }, default);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Page, Is.EqualTo(2));
        Assert.That(result.Value.PageSize, Is.EqualTo(pageSize));
        Assert.That(result.Value.TotalPages, Is.EqualTo(totalPages));
        Assert.That(repository.Calls, Is.EqualTo(2));
    }

    [Test]
    public void CompatibilityRoundsMidpointsUpConsistentlyWithPostgres()
    {
        var skills = Enumerable.Range(0, 16).Select(_ => Guid.NewGuid()).ToArray();
        var mentor = Mentor();
        mentor.Skills.Add(new QuestionnaireSkill { QuestionnaireId = mentor.Id, SkillId = skills[0] });
        mentor.Availability.Add(Window(mentor.Id, DayOfWeek.Monday));
        var score = new MentorCompatibilityService().CreateProjection(new MentorPreferences
        {
            SkillIds = skills, DaysOfWeek = [DayOfWeek.Monday], MinMonths = 1, MaxMonths = 3,
            HoursPerWeek = 2, CommunicationFormat = CommunicationFormat.Online
        }).Compile();

        // 40 * 1/16 + 20 + 20 + 10 + 10 = 62.5, which must become 63 in both runtimes.
        Assert.That(score(mentor).Compatibility, Is.EqualTo(63));
    }

    [Test]
    public void CompatibilityIsNormalizedAndDoesNotCountDuplicateWindows()
    {
        var mentor = Mentor();
        var skill = Guid.NewGuid();
        mentor.Skills.Add(new QuestionnaireSkill { SkillId = skill, QuestionnaireId = mentor.Id });
        mentor.Availability.AddRange([Window(mentor.Id, DayOfWeek.Monday), Window(mentor.Id, DayOfWeek.Monday)]);
        var score = new MentorCompatibilityService().CreateProjection(new MentorPreferences
        {
            SkillIds = [skill, Guid.NewGuid()], DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Tuesday],
            MinMonths = 1, MaxMonths = 3, HoursPerWeek = 2, CommunicationFormat = CommunicationFormat.Online
        }).Compile();
        Assert.That(score(mentor).Compatibility, Is.EqualTo(70));
        Assert.That(new MentorCompatibilityService().CreateProjection(null).Compile()(mentor).Compatibility, Is.Null);
        var optional = new MentorCompatibilityService().CreateProjection(new MentorPreferences
        {
            MinMonths = 1, MaxMonths = 3, HoursPerWeek = 2, CommunicationFormat = CommunicationFormat.Online
        }).Compile();
        Assert.That(optional(mentor).Compatibility, Is.EqualTo(100));
    }

    private static MentorSearchService CreateService(SearchRepository repository) =>
        new(repository, new MentorCompatibilityService());

    private static Questionnaire Mentor() => new()
    {
        UserId = Guid.NewGuid(), FirstName = "Ирина", LastName = "Ли", City = "Пермь", Education = "Вуз",
        GoalDescription = "Обучать", MonthsNumberMin = 1, MonthsNumberMax = 3, HoursNumberWeek = 2,
        CommunicationFormat = CommunicationFormat.Hybrid
    };

    private static AvailabilitySlot Window(Guid questionnaireId, DayOfWeek day) => new()
    {
        QuestionnaireId = questionnaireId, DayOfWeek = day, StartTime = new(10, 0), EndTime = new(11, 0)
    };

    private sealed class SearchRepository : IMentorSearchRepository
    {
        public int Calls { get; private set; }
        public Task<MentorPreferences?> GetPreferencesAsync(Guid userId, CancellationToken ct)
        {
            Calls++;
            return Task.FromResult<MentorPreferences?>(null);
        }

        public Task<MentorSearchResult> SearchAsync(MentorSearchRequest request,
            Expression<Func<Questionnaire, MentorMatch>> compatibility, CancellationToken ct)
        {
            Calls++;
            return Task.FromResult(new MentorSearchResult([], 21, request.Page, request.PageSize));
        }
    }
}
