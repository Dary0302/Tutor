using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tutor.Application.Configurations;
using Tutor.Application.Goals;
using Tutor.Domain.Mentorship;
using Tutor.Infrastructure;
using Tutor.Infrastructure.Repositories;

namespace Tutor.Tests.Infrastructure;

public sealed class GoalQueryTests
{
    private ServiceProvider services = null!;
    private TutorDbContext db = null!;

    [SetUp]
    public void SetUp()
    {
        services = new ServiceCollection().AddLogging().AddApplicationServices().BuildServiceProvider();
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
    public void GoalCardsAreScopedAndPagedInSqlWithOnlyTheirTasks()
    {
        var request = new GoalListRequest { PairId = Guid.NewGuid(), Page = 3, PageSize = 7, Status = GoalStatus.PendingReview };
        var query = GoalRepository.GoalPageQuery(GoalRepository.FilterGoals(db, Guid.NewGuid(), request), request,
            services.GetRequiredService<IMapper>().ConfigurationProvider);
        var sql = query.ToQueryString();
        Assert.That(sql, Does.Contain("LIMIT").And.Contain("OFFSET").And.Contain("ORDER BY"));
        Assert.That(sql, Does.Contain("teacher_id").And.Contain("student_id").And.Contain("DeletedAt"));
        Assert.That(sql, Does.Contain("PendingReview").And.Contain("position").And.Contain("tasks"));
        Assert.That(sql, Does.Not.Contain("Password").And.Not.Contain("goal_description"));
    }

    [Test]
    public void ProgressUsesSqlAggregationAcrossAllPagesAndExcludesCancelledGoals()
    {
        var sql = GoalRepository.ProgressQuery(db, Guid.NewGuid(), Guid.NewGuid()).ToQueryString();
        Assert.That(sql.ToLowerInvariant(), Does.Contain("group by").And.Contain("sum(").And.Contain("count("));
        Assert.That(sql, Does.Contain("Cancelled").And.Contain("Completed").And.Contain("teacher_id").And.Contain("student_id"));
        Assert.That(sql, Does.Not.Contain("LIMIT").And.Not.Contain("OFFSET"));
    }

    [Test]
    public void StudentLookupIsScopedToTheMentorsActivePairs()
    {
        var sql = GoalRepository.StudentQuery(db, Guid.NewGuid()).Take(10).ToQueryString();
        Assert.That(sql, Does.Contain("Active").And.Contain("teacher_id").And.Contain("first_name"));
        Assert.That(sql, Does.Not.Contain("Password"));
    }
}
