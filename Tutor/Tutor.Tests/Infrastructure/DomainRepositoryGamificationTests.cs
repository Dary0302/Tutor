using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tutor.Application.Gamification;
using Tutor.Application.Goals;
using Tutor.Application.Sessions;
using Tutor.Domain.Mentorship;
using Tutor.Domain.Questionnaires;
using Tutor.Domain.Users;

namespace Tutor.Tests.Infrastructure;

public sealed partial class DomainRepositoryTests
{
    [Test]
    public async Task RatingTracksRecordedActivityForEachRole()
    {
        var (pair, mentor, student, skill) = await CreateGoalPair();
        var sessions = scope!.ServiceProvider.GetRequiredService<ISessionService>();
        var goals = scope.ServiceProvider.GetRequiredService<IGoalService>();
        var gamification = scope.ServiceProvider.GetRequiredService<IGamificationRepository>();
        var points = new GamificationOptions
        {
            Student = new() { SessionPoints = 20, TaskPoints = 10, GoalPoints = 40, QuestionnairePoints = 15 },
            Mentor = new() { SessionPoints = 25, StudentGoalPoints = 50, QuestionnairePoints = 15 }
        };
        var now = DateTimeOffset.UtcNow;
        async Task<MentoringSession> AddSession(DateTimeOffset startsAt, SessionStatus status = SessionStatus.Scheduled)
        {
            var session = new MentoringSession
            {
                PairId = pair.Id, Agenda = "Практика", Status = status,
                StartsAt = startsAt, EndsAt = startsAt.AddMinutes(30)
            };
            await Repository<MentoringSession>().AddAsync(session);
            return session;
        }

        var held = await AddSession(now.AddHours(-2));
        var future = await AddSession(now.AddHours(2));
        var cancelled = await AddSession(now.AddHours(-4), SessionStatus.Cancelled);
        Assert.That((await sessions.CompleteAsync(mentor, future.Id, default)).IsFailed, Is.True);
        Assert.That((await sessions.CancelAsync(mentor, future.Id, default)).IsSuccess, Is.True);
        Assert.That((await Repository<MentoringSession>().GetByIdAsync(future.Id))!.Status, Is.EqualTo(SessionStatus.Cancelled));
        Assert.That((await sessions.CompleteAsync(mentor, cancelled.Id, default)).IsFailed, Is.True);
        var stranger = await CreateUser(UserRole.Mentor);
        Assert.That((await sessions.CompleteAsync(stranger, held.Id, default)).IsFailed, Is.True);
        var completions = await Task.WhenAll(sessions.CompleteAsync(mentor, held.Id, default),
            sessions.CompleteAsync(mentor, held.Id, default));
        Assert.That(completions.All(result => result.IsSuccess), Is.True);

        var goal = (await goals.CreateAsync(mentor, new()
        {
            PairId = pair.Id, SkillId = skill.Id, Title = "Учебная цель", Tasks = [new() { Name = "Задание" }]
        }, default)).Value;
        var taskId = goal.Tasks.Single().Id;
        Assert.That((await goals.SetTaskCompletionAsync(student, goal.Id, taskId, true, default)).IsSuccess, Is.True);
        Assert.That((await gamification.GetProgressAsync(student, UserRole.Student, points, default))!.Points, Is.EqualTo(30));
        Assert.That((await gamification.GetProgressAsync(mentor, UserRole.Mentor, points, default))!.Points, Is.EqualTo(25));
        Assert.That((await goals.SetTaskCompletionAsync(student, goal.Id, taskId, false, default)).IsSuccess, Is.True);
        Assert.That((await gamification.GetProgressAsync(student, UserRole.Student, points, default))!.Points, Is.EqualTo(20));
        Assert.That((await goals.SetTaskCompletionAsync(student, goal.Id, taskId, true, default)).IsSuccess, Is.True);
        Assert.That((await goals.ConfirmAsync(mentor, goal.Id, default)).IsSuccess, Is.True);
        await Repository<Questionnaire>().AddAsync(new()
        {
            UserId = student, FirstName = "Анна", LastName = "Ли", City = "Пермь", Education = "Университет",
            GoalDescription = "Учиться"
        });

        var studentProgress = (await gamification.GetProgressAsync(student, UserRole.Student, points, default))!;
        var mentorProgress = (await gamification.GetProgressAsync(mentor, UserRole.Mentor, points, default))!;
        Assert.That((studentProgress.Points, studentProgress.Sessions, studentProgress.Tasks, studentProgress.Goals,
            studentProgress.HasQuestionnaire), Is.EqualTo((85L, 1L, 1L, 1L, true)));
        Assert.That((mentorProgress.Points, mentorProgress.Sessions, mentorProgress.Tasks, mentorProgress.Goals),
            Is.EqualTo((75L, 1L, 0L, 1L)));
        Assert.That((await gamification.GetProgressAsync(stranger, UserRole.Mentor, points, default))!.Rank, Is.Null);

        var students = await gamification.GetLeadersAsync(UserRole.Student, 100, points, default);
        var mentors = await gamification.GetLeadersAsync(UserRole.Mentor, 100, points, default);
        Assert.That(students.Single(entry => entry.UserId == student), Is.EqualTo(new LeaderboardScore(student, "Анна", "Ли", 85)));
        Assert.That(studentProgress.Rank, Is.EqualTo(students.Count(entry => entry.Points > 85) + 1));
        Assert.That(mentors.Select(entry => entry.UserId), Does.Contain(mentor).And.Not.Contain(student).And.Not.Contain(stranger));

        var changedPoints = new GamificationOptions { Student = new() { SessionPoints = 100 } };
        Assert.That((await gamification.GetProgressAsync(student, UserRole.Student, changedPoints, default))!.Points, Is.EqualTo(100));
    }
}
