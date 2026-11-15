using Core.Common.Errors;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tutor.Application.Configurations;
using Tutor.Application.Goals;
using Tutor.Domain.Mentorship;
using Tutor.Tests.TestDoubles;

namespace Tutor.Tests.Application;

public sealed class GoalServiceTests
{
    private ServiceProvider services = null!;
    private GoalTestRepository repository = null!;
    private IGoalService goals = null!;
    private IGoalProgressService progress = null!;

    [SetUp]
    public void SetUp()
    {
        services = new ServiceCollection().AddLogging().AddApplicationServices()
            .AddSingleton<IGoalRepository, GoalTestRepository>().BuildServiceProvider();
        repository = (GoalTestRepository)services.GetRequiredService<IGoalRepository>();
        goals = services.GetRequiredService<IGoalService>();
        progress = services.GetRequiredService<IGoalProgressService>();
    }

    [TearDown]
    public void TearDown() => services.Dispose();

    [Test]
    public async Task LastTaskSendsGoalToReviewAndOnlyMentorConfirmsIt()
    {
        var created = (await goals.CreateAsync(repository.Pair.MentorId, repository.NewGoal(), default)).Value;
        Assert.That(created.Title, Is.EqualTo("Сверстать дашборд"));
        Assert.That(created.Tasks.Select(t => t.Position), Is.EqualTo(new[] { 0, 1 }));
        Assert.That(created.Tasks[0].Name, Is.EqualTo("Задача 1"));
        Assert.That(created.Status, Is.EqualTo(GoalStatus.Active));
        Assert.That((await goals.ConfirmAsync(repository.Pair.MentorId, created.Id, default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.Conflict));

        var first = (await goals.SetTaskCompletionAsync(repository.Pair.StudentId, created.Id, created.Tasks[0].Id, true, default)).Value;
        Assert.That(first.Status, Is.EqualTo(GoalStatus.Active));
        Assert.That((await progress.GetAsync(repository.Pair.StudentId, null, default)).Single().Percentage, Is.EqualTo(50));
        var reviewed = (await goals.SetTaskCompletionAsync(repository.Pair.StudentId, created.Id, created.Tasks[1].Id, true, default)).Value;
        Assert.That(reviewed.Status, Is.EqualTo(GoalStatus.PendingReview));
        Assert.That(reviewed.ClosedAt, Is.Null);
        Assert.That((await progress.GetAsync(repository.Pair.MentorId, repository.Pair.Id, default)).Single().Percentage, Is.EqualTo(100));
        Assert.That((await goals.ConfirmAsync(repository.Pair.StudentId, created.Id, default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.Forbidden));

        var confirmed = (await goals.ConfirmAsync(repository.Pair.MentorId, created.Id, default)).Value;
        Assert.That(confirmed.Status, Is.EqualTo(GoalStatus.Completed));
        Assert.That(confirmed.ClosedAt, Is.Not.Null);
        Assert.That((await goals.ConfirmAsync(repository.Pair.MentorId, created.Id, default)).Value.ClosedAt, Is.EqualTo(confirmed.ClosedAt));
        Assert.That((await goals.SetTaskCompletionAsync(repository.Pair.StudentId, created.Id, created.Tasks[0].Id, false, default))
            .GetAppError().Status, Is.EqualTo(ErrorStatus.Conflict));
    }

    [Test]
    public async Task UndoAndNewTasksReopenReviewButRepeatedCompletionPreservesTimestamp()
    {
        var goal = (await goals.CreateAsync(repository.Pair.MentorId, repository.NewGoal(1), default)).Value;
        var done = (await goals.SetTaskCompletionAsync(repository.Pair.StudentId, goal.Id, goal.Tasks[0].Id, true, default)).Value;
        var again = (await goals.SetTaskCompletionAsync(repository.Pair.StudentId, goal.Id, goal.Tasks[0].Id, true, default)).Value;
        Assert.That(again.Tasks[0].CompletedAt, Is.EqualTo(done.Tasks[0].CompletedAt));
        var undone = (await goals.SetTaskCompletionAsync(repository.Pair.StudentId, goal.Id, goal.Tasks[0].Id, false, default)).Value;
        Assert.That(undone.Status, Is.EqualTo(GoalStatus.Active));
        Assert.That(undone.Tasks[0].CompletedAt, Is.Null);
        await goals.SetTaskCompletionAsync(repository.Pair.StudentId, goal.Id, goal.Tasks[0].Id, true, default);
        var extended = (await goals.AddTaskAsync(repository.Pair.MentorId, goal.Id, new() { Name = "Проверить доступность" }, default)).Value;
        Assert.That(extended.Status, Is.EqualTo(GoalStatus.Active));
        Assert.That(extended.CompletedTasks, Is.EqualTo(1));
        var reduced = (await goals.RemoveTaskAsync(repository.Pair.MentorId, goal.Id, extended.Tasks[1].Id, default)).Value;
        Assert.That(reduced.Status, Is.EqualTo(GoalStatus.PendingReview));
        Assert.That((await goals.RemoveTaskAsync(repository.Pair.MentorId, goal.Id, goal.Tasks[0].Id, default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.Conflict));
    }

    [Test]
    public async Task OwnershipAndRoleAreCheckedBeforeChangingData()
    {
        Assert.That((await goals.CreateAsync(repository.Pair.StudentId, repository.NewGoal(), default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.Forbidden));
        Assert.That((await goals.CreateAsync(Guid.NewGuid(), repository.NewGoal(), default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.NotFound));
        var goal = (await goals.CreateAsync(repository.Pair.MentorId, repository.NewGoal(), default)).Value;
        var saves = repository.SaveCount;
        Assert.That((await goals.AddTaskAsync(repository.Pair.StudentId, goal.Id, new() { Name = "X" }, default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.Forbidden));
        Assert.That((await goals.SetTaskCompletionAsync(repository.Pair.MentorId, goal.Id, goal.Tasks[0].Id, true, default))
            .GetAppError().Status, Is.EqualTo(ErrorStatus.Forbidden));
        Assert.That((await goals.SetTaskCompletionAsync(Guid.NewGuid(), goal.Id, goal.Tasks[0].Id, true, default))
            .GetAppError().Status, Is.EqualTo(ErrorStatus.NotFound));
        Assert.That((await goals.SetTaskCompletionAsync(repository.Pair.StudentId, goal.Id, Guid.NewGuid(), true, default))
            .GetAppError().Status, Is.EqualTo(ErrorStatus.NotFound));
        Assert.That(repository.SaveCount, Is.EqualTo(saves));
        Assert.That(repository.Goal!.Tasks.All(t => t.Status == GoalTaskStatus.Pending), Is.True);
    }

    [TestCase(PairStatus.Completed)]
    [TestCase(PairStatus.Terminated)]
    public async Task InactivePairsRemainReadableButCannotBeChanged(PairStatus status)
    {
        var goal = (await goals.CreateAsync(repository.Pair.MentorId, repository.NewGoal(), default)).Value;
        repository.Pair = repository.Pair with { Status = status };
        var saves = repository.SaveCount;
        Assert.That((await goals.GetAsync(repository.Pair.StudentId, goal.Id, default)).IsSuccess, Is.True);
        Assert.That((await goals.CreateAsync(repository.Pair.MentorId, repository.NewGoal(), default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.Conflict));
        Assert.That((await goals.SetTaskCompletionAsync(repository.Pair.StudentId, goal.Id, goal.Tasks[0].Id, true, default))
            .GetAppError().Status, Is.EqualTo(ErrorStatus.Conflict));
        Assert.That(repository.SaveCount, Is.EqualTo(saves));
    }

    [Test]
    public async Task UnknownSkillReturnsValidationErrorAndDisposesWriteRepository()
    {
        var request = new CreateGoalRequest
        {
            PairId = repository.Pair.Id, SkillId = Guid.NewGuid(), Title = "HTML", Tasks = [new() { Name = "Задача" }]
        };
        var result = await goals.CreateAsync(repository.Pair.MentorId, request, default);
        Assert.That(result.GetAppError().Status, Is.EqualTo(ErrorStatus.Validation));
        Assert.That(repository.SaveCount, Is.Zero);
        Assert.That(repository.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public async Task CancelledGoalsDisappearFromListAndProgressAndCannotBeChanged()
    {
        var goal = (await goals.CreateAsync(repository.Pair.MentorId, repository.NewGoal(), default)).Value;
        Assert.That((await goals.CancelAsync(repository.Pair.MentorId, goal.Id, default)).IsSuccess, Is.True);
        Assert.That((await goals.ListAsync(repository.Pair.StudentId, new(), default)).Items, Is.Empty);
        Assert.That(await progress.GetAsync(repository.Pair.StudentId, null, default), Is.Empty);
        Assert.That((await goals.GetAsync(repository.Pair.StudentId, goal.Id, default)).GetAppError().Status, Is.EqualTo(ErrorStatus.NotFound));
        Assert.That((await goals.AddTaskAsync(repository.Pair.MentorId, goal.Id, new() { Name = "X" }, default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.Conflict));
        Assert.That((await goals.CancelAsync(repository.Pair.MentorId, goal.Id, default)).IsSuccess, Is.True);
    }

    [Test]
    public async Task TaskLimitIsEnforcedWhenAppendingAndProgressRoundsToWholePercent()
    {
        var goal = (await goals.CreateAsync(repository.Pair.MentorId, repository.NewGoal(100), default)).Value;
        Assert.That((await goals.AddTaskAsync(repository.Pair.MentorId, goal.Id, new() { Name = "X" }, default)).GetAppError().Status,
            Is.EqualTo(ErrorStatus.Conflict));
        repository.Reset();
        goal = (await goals.CreateAsync(repository.Pair.MentorId, repository.NewGoal(3), default)).Value;
        await goals.SetTaskCompletionAsync(repository.Pair.StudentId, goal.Id, goal.Tasks[0].Id, true, default);
        var summary = (await progress.GetAsync(repository.Pair.StudentId, null, default)).Single();
        Assert.That(summary.TotalTasks, Is.EqualTo(3));
        Assert.That(summary.CompletedTasks, Is.EqualTo(1));
        Assert.That(summary.Percentage, Is.EqualTo(33));
    }
}
