using AutoMapper;
using System.Text.RegularExpressions;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tutor.Application.Configurations;
using Tutor.Application.Mentors;
using Tutor.Domain.Questionnaires;
using Tutor.Infrastructure;
using Tutor.Infrastructure.Repositories;

namespace Tutor.Tests.Infrastructure;

public sealed class MentorSearchQueryTests
{
    private ServiceProvider services = null!;
    private TutorDbContext db = null!;
    private IConfigurationProvider mappings = null!;

    [SetUp]
    public void SetUp()
    {
        services = new ServiceCollection().AddLogging().AddApplicationServices().BuildServiceProvider();
        mappings = services.GetRequiredService<IMapper>().ConfigurationProvider;
        db = new TutorDbContext(new DbContextOptionsBuilder<TutorDbContext>()
            .UseNpgsql("Host=localhost;Database=translation_only").Options);
    }

    [TearDown]
    public void TearDown()
    {
        db.Dispose();
        services.Dispose();
    }

    [Test]
    public void AutoMapperConfigurationIsValid() => mappings.AssertConfigurationIsValid();

    [TestCase(MentorSort.Compatibility, true)]
    [TestCase(MentorSort.Compatibility, false)]
    [TestCase(MentorSort.Rating, true)]
    [TestCase(MentorSort.Experience, true)]
    [TestCase(MentorSort.HoursPerWeek, true)]
    public void FiltersScoreOrderingAndPaginationTranslateToPostgres(MentorSort sort, bool hasPreferences)
    {
        var skill = Guid.NewGuid();
        var request = new MentorSearchRequest
        {
            SkillIds = [skill, Guid.NewGuid()], MinYearsExperience = 2, MaxYearsExperience = 10,
            DurationMonths = [1, 3], DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Friday],
            HasAvailablePlaces = true, MinHoursPerWeek = 1.5m, MaxHoursPerWeek = 5,
            MinRating = 4.5m, PlaceOfWork = "  Яндекс%_  ", Sort = sort, Page = 2, PageSize = 7
        };
        var preferences = hasPreferences ? new MentorPreferences
        {
            SkillIds = [skill], DaysOfWeek = [DayOfWeek.Monday], MinMonths = 1, MaxMonths = 6,
            HoursPerWeek = 2, CommunicationFormat = CommunicationFormat.Online
        } : null;
        var score = new MentorCompatibilityService().CreateProjection(preferences);
        var filtered = MentorSearchRepository.FilterMentors(db, request);
        var sql = MentorSearchRepository.BuildPageQuery(db, filtered, request, score, mappings).ToQueryString();

        Assert.That(sql, Does.Contain("LIMIT").And.Contain("OFFSET").And.Contain("ORDER BY"));
        Assert.That(sql, Does.Contain("ILIKE").And.Contain("EXISTS").And.Contain("Active").And.Contain("Mentor"));
        Assert.That(sql, Does.Contain("DeletedAt").And.Contain("rating").And.Contain("day_of_week"));
        Assert.That(sql, Does.Not.Contain("goal_description").And.Not.Contain("education").And.Not.Contain("Password"));
        if (hasPreferences)
            Assert.That(sql, Does.Contain("floor("));
    }

    [Test]
    public void PreferencesProjectionTranslatesToPostgres()
    {
        var sql = db.Set<Questionnaire>().Where(q => q.UserId == Guid.Empty)
            .ProjectTo<MentorPreferences>(mappings).ToQueryString();
        Assert.That(sql, Does.Contain("skill_id").And.Contain("day_of_week"));
        Assert.That(sql, Does.Not.Contain("goal_description"));
    }

    [TestCase(1)]
    [TestCase(7)]
    [TestCase(100)]
    public void SqlPaginationUsesRequestedSizeAndOffset(int pageSize)
    {
        var request = new MentorSearchRequest { Page = 3, PageSize = pageSize };
        var score = new MentorCompatibilityService().CreateProjection(null);
        var query = MentorSearchRepository.BuildPageQuery(db, MentorSearchRepository.FilterMentors(db, request),
            request, score, mappings);
        using var command = query.CreateDbCommand();
        var pagination = Regex.Match(command.CommandText, @"LIMIT @(?<limit>\w+) OFFSET @(?<offset>\w+)");

        Assert.That(pagination.Success, Is.True, command.CommandText);
        Assert.That(command.Parameters[pagination.Groups["limit"].Value].Value, Is.EqualTo(pageSize));
        Assert.That(command.Parameters[pagination.Groups["offset"].Value].Value, Is.EqualTo(2 * pageSize));
    }
}
