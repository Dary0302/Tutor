using Core.Common.Errors;
using FluentResults;
using Microsoft.Extensions.Options;
using Tutor.Domain.Users;

namespace Tutor.Application.Gamification;

public sealed class GamificationService(IGamificationRepository repository, IOptionsMonitor<GamificationOptions> options)
{
    public async Task<Result<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboardAsync(UserRole role,
        LeaderboardRequest request, CancellationToken ct)
    {
        if (role is not (UserRole.Student or UserRole.Mentor))
            return Result.Fail(AppError.Forbidden());

        var leaders = await repository.GetLeadersAsync(role, request.Limit, options.CurrentValue, ct);
        var entries = new List<LeaderboardEntryDto>(leaders.Count);
        long rank = 0;
        for (var i = 0; i < leaders.Count; i++)
        {
            var leader = leaders[i];
            if (i == 0 || leader.Points != leaders[i - 1].Points)
                rank = i + 1;
            entries.Add(new LeaderboardEntryDto(leader.UserId, leader.FirstName, leader.LastName, leader.Points, rank));
        }

        return Result.Ok<IReadOnlyList<LeaderboardEntryDto>>(entries);
    }

    public async Task<Result<GamificationProfileDto>> GetProfileAsync(Guid userId, UserRole role, CancellationToken ct)
    {
        if (role is not (UserRole.Student or UserRole.Mentor))
            return Result.Fail(AppError.Forbidden());

        var progress = await repository.GetProgressAsync(userId, role, options.CurrentValue, ct);
        if (progress is null)
            return Result.Fail(AppError.NotFound("Профиль геймификации не найден."));

        return Result.Ok(new GamificationProfileDto(progress.Points, progress.Rank, GetAchievements(role, progress)));
    }

    private static List<AchievementDto> GetAchievements(UserRole role, GamificationProgress progress)
    {
        var achievements = new List<AchievementDto>();
        if (progress.HasQuestionnaire)
            achievements.Add(new("profile_completed", "Знакомство", "Заполнить анкету участника"));
        if (progress.Sessions >= 1)
            achievements.Add(new("first_session", "Первая сессия", "Завершить одну сессию"));
        if (progress.Sessions >= 5)
            achievements.Add(new("five_sessions", "В рабочем ритме", "Завершить пять сессий"));
        if (role == UserRole.Student && progress.Tasks >= 5)
            achievements.Add(new("five_tasks", "Практика", "Выполнить пять заданий"));
        if (progress.Goals >= 1)
            achievements.Add(role == UserRole.Student
                ? new("first_goal", "Первая цель", "Выполнить цель и получить подтверждение наставника")
                : new("first_student_goal", "Первый результат", "Помочь студенту выполнить цель"));
        if (progress.Goals >= 3)
            achievements.Add(role == UserRole.Student
                ? new("three_goals", "Три цели", "Выполнить три цели и получить подтверждение наставника")
                : new("three_student_goals", "Три результата", "Помочь студентам выполнить три цели"));
        return achievements;
    }
}
