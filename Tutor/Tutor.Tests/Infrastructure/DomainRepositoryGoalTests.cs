using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tutor.Application.Goals;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Users;
using Tutor.Infrastructure;

namespace Tutor.Tests.Infrastructure;

public sealed partial class DomainRepositoryTests
{
    [Test]
    public async Task GoalsPersistWithTasksAndProgressAcrossPagesAndExcludeOtherPairs()
    {
        var (pair, mentor, student, skill) = await CreateGoalPair();
        var service = scope!.ServiceProvider.GetRequiredService<IGoalService>();
        var progress = scope.ServiceProvider.GetRequiredService<IGoalProgressService>();
        CreateGoalRequest Request(string title) => new()
        {
            PairId = pair.Id, SkillId = skill.Id, Title = title,
            Tasks = [new() { Name = "Первая" }, new() { Name = "Вторая" }]
        };
        var first = (await service.CreateAsync(mentor, Request("Первая цель"), default)).Value;
        var second = (await service.CreateAsync(mentor, Request("Вторая цель"), default)).Value;
        var updated = (await service.SetTaskCompletionAsync(student, first.Id, first.Tasks[0].Id, true, default)).Value;
        Assert.That(updated.CompletedTasks, Is.EqualTo(1));
        var page = await service.ListAsync(student, new() { PairId = pair.Id, PageSize = 1 }, default);
        Assert.That(page.TotalCount, Is.EqualTo(2));
        Assert.That(page.Items, Has.Count.EqualTo(1));
        var summary = (await progress.GetAsync(student, pair.Id, default)).Single();
        Assert.That(summary.TotalTasks, Is.EqualTo(4));
        Assert.That(summary.CompletedTasks, Is.EqualTo(1));
        Assert.That(summary.Percentage, Is.EqualTo(25));

        var stranger = await CreateUser(UserRole.Student);
        Assert.That((await service.ListAsync(stranger, new() { PairId = pair.Id }, default)).Items, Is.Empty);
        Assert.That(await progress.GetAsync(stranger, pair.Id, default), Is.Empty);
        Assert.That((await service.SetTaskCompletionAsync(stranger, first.Id, first.Tasks[1].Id, true, default)).IsFailed, Is.True);
        Assert.That((await service.SetTaskCompletionAsync(student, first.Id, second.Tasks[0].Id, true, default)).IsFailed, Is.True);

        await service.SetTaskCompletionAsync(student, first.Id, first.Tasks[1].Id, true, default);
        Assert.That((await service.GetAsync(mentor, first.Id, default)).Value.Status, Is.EqualTo(GoalStatus.PendingReview));
        await service.ConfirmAsync(mentor, first.Id, default);
        var confirmed = (await service.GetAsync(student, first.Id, default)).Value;
        Assert.That(confirmed.Status, Is.EqualTo(GoalStatus.Completed));
        Assert.That(confirmed.ClosedAt, Is.Not.Null);
        Assert.That((await service.CancelAsync(mentor, second.Id, default)).IsSuccess, Is.True);
        summary = (await progress.GetAsync(student, pair.Id, default)).Single();
        Assert.That(summary.TotalTasks, Is.EqualTo(2));
        Assert.That(summary.Percentage, Is.EqualTo(100));
    }

    [Test]
    public async Task ConcurrentTaskCompletionAndConfirmationKeepGoalStateConsistent()
    {
        var (pair, mentor, student, skill) = await CreateGoalPair();
        var service = scope!.ServiceProvider.GetRequiredService<IGoalService>();
        var goal = (await service.CreateAsync(mentor, new()
        {
            PairId = pair.Id, SkillId = skill.Id, Title = "Параллельное выполнение",
            Tasks = [new() { Name = "Первая" }, new() { Name = "Вторая" }]
        }, default)).Value;
        var results = await Task.WhenAll(goal.Tasks.Select(t => service.SetTaskCompletionAsync(student, goal.Id, t.Id, true, default)));
        Assert.That(results.All(r => r.IsSuccess), Is.True);
        var reviewed = (await service.GetAsync(student, goal.Id, default)).Value;
        Assert.That(reviewed.Status, Is.EqualTo(GoalStatus.PendingReview));
        Assert.That(reviewed.CompletedTasks, Is.EqualTo(2));

        await Task.WhenAll(
            service.ConfirmAsync(mentor, goal.Id, default),
            service.AddTaskAsync(mentor, goal.Id, new() { Name = "Дополнительная проверка" }, default));
        var final = (await service.GetAsync(student, goal.Id, default)).Value;
        Assert.That(final.Status == GoalStatus.Completed && final.TotalTasks == 2 ||
            final.Status == GoalStatus.Active && final.TotalTasks == 3, Is.True);
    }

    [Test]
    public async Task RemovingTaskPersistsAndFailedMutationReleasesWriteLock()
    {
        var (pair, mentor, student, skill) = await CreateGoalPair();
        var service = scope!.ServiceProvider.GetRequiredService<IGoalService>();
        var goal = (await service.CreateAsync(mentor, new()
        {
            PairId = pair.Id, SkillId = skill.Id, Title = "Удаление задачи",
            Tasks = [new() { Name = "Первая" }, new() { Name = "Вторая" }]
        }, default)).Value;
        Assert.That((await service.RemoveTaskAsync(student, goal.Id, goal.Tasks[0].Id, default)).IsFailed, Is.True);
        Assert.That((await service.RemoveTaskAsync(mentor, goal.Id, goal.Tasks[0].Id, default)).IsSuccess, Is.True);
        Assert.That(await Repository<GoalTask>().GetByIdAsync(goal.Tasks[0].Id), Is.Null);
        Assert.That((await service.RemoveTaskAsync(mentor, goal.Id, goal.Tasks[1].Id, default)).IsFailed, Is.True);
        Assert.That((await service.GetAsync(student, goal.Id, default)).Value.TotalTasks, Is.EqualTo(1));
    }

    [Test]
    public async Task EndedPairsAndDeletedUsersCannotWriteGoals()
    {
        var (pair, mentor, student, skill) = await CreateGoalPair();
        var service = scope!.ServiceProvider.GetRequiredService<IGoalService>();
        var goal = (await service.CreateAsync(mentor, new()
        {
            PairId = pair.Id, SkillId = skill.Id, Title = "Доступ",
            Tasks = [new() { Name = "Задача" }]
        }, default)).Value;
        pair.Status = PairStatus.Terminated;
        await Repository<MentorshipPair>().UpdateAsync(pair);
        Assert.That((await service.SetTaskCompletionAsync(student, goal.Id, goal.Tasks[0].Id, true, default)).IsFailed, Is.True);
        Assert.That((await service.GetAsync(student, goal.Id, default)).IsSuccess, Is.True);
        var factory = scope!.ServiceProvider.GetRequiredService<IDbContextFactory<TutorDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Users.Where(u => u.Id == student).ExecuteUpdateAsync(set => set.SetProperty(u => u.DeletedAt, DateTimeOffset.UtcNow));
        Assert.That((await service.GetAsync(student, goal.Id, default)).IsFailed, Is.True);
    }

    private async Task<(MentorshipPair Pair, Guid Mentor, Guid Student, Skill Skill)> CreateGoalPair()
    {
        var mentor = await CreateUser(UserRole.Mentor);
        var student = await CreateUser(UserRole.Student);
        var request = new MentorshipRequest
        {
            TeacherId = mentor, StudentId = student, StudentLetter = "Учиться", Status = RequestStatus.Accepted
        };
        await Repository<MentorshipRequest>().AddAsync(request);
        var pair = new MentorshipPair { RequestId = request.Id };
        await Repository<MentorshipPair>().AddAsync(pair);
        var skill = new Skill { Name = $"HTML-{Guid.NewGuid():N}" };
        await Repository<Skill>().AddAsync(skill);
        return (pair, mentor, student, skill);
    }
}
