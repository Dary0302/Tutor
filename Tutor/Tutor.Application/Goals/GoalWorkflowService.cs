using Core.Common.Errors;
using FluentResults;
using Tutor.Domain.Mentorship;

namespace Tutor.Application.Goals;

public sealed class GoalWorkflowService(TimeProvider timeProvider) : IGoalWorkflowService
{
    public Result AddTask(LearningGoal goal, GoalTask task)
    {
        if (IsClosed(goal))
            return ClosedGoal();
        if (goal.Tasks.Count >= GoalLimits.MaxTasksPerGoal)
            return Result.Fail(AppError.Conflict("Достигнуто максимальное количество задач в цели."));

        task.GoalId = goal.Id;
        task.Position = goal.Tasks.Count == 0 ? 0 : goal.Tasks.Max(x => x.Position) + 1;
        goal.Tasks.Add(task);
        RefreshReviewStatus(goal);
        return Result.Ok();
    }

    public Result RemoveTask(LearningGoal goal, Guid taskId)
    {
        if (IsClosed(goal))
            return ClosedGoal();
        var task = goal.Tasks.SingleOrDefault(x => x.Id == taskId);
        if (task is null)
            return Result.Fail(AppError.NotFound("Задача не найдена."));
        if (goal.Tasks.Count == 1)
            return Result.Fail(AppError.Conflict("В цели должна оставаться хотя бы одна задача."));

        goal.Tasks.Remove(task);
        RefreshReviewStatus(goal);
        return Result.Ok();
    }

    public Result SetTaskCompletion(LearningGoal goal, Guid taskId, bool completed)
    {
        if (IsClosed(goal))
            return ClosedGoal();
        var task = goal.Tasks.SingleOrDefault(x => x.Id == taskId);
        if (task is null)
            return Result.Fail(AppError.NotFound("Задача не найдена."));
        if ((task.Status == GoalTaskStatus.Completed) != completed)
        {
            task.Status = completed ? GoalTaskStatus.Completed : GoalTaskStatus.Pending;
            task.CompletedAt = completed ? timeProvider.GetUtcNow() : null;
        }
        RefreshReviewStatus(goal);
        return Result.Ok();
    }

    public Result Confirm(LearningGoal goal)
    {
        if (goal.Status == GoalStatus.Completed)
            return Result.Ok();
        if (goal.Status != GoalStatus.PendingReview || goal.Tasks.Count == 0 ||
            goal.Tasks.Any(x => x.Status != GoalTaskStatus.Completed))
            return Result.Fail(AppError.Conflict("Подтвердить можно только цель на проверке со всеми выполненными задачами."));

        goal.Status = GoalStatus.Completed;
        goal.ClosedAt = timeProvider.GetUtcNow();
        return Result.Ok();
    }

    public Result Cancel(LearningGoal goal)
    {
        if (goal.Status == GoalStatus.Cancelled)
            return Result.Ok();
        if (goal.Status == GoalStatus.Completed)
            return ClosedGoal();
        goal.Status = GoalStatus.Cancelled;
        goal.ClosedAt = timeProvider.GetUtcNow();
        return Result.Ok();
    }

    private static bool IsClosed(LearningGoal goal) => goal.Status is GoalStatus.Completed or GoalStatus.Cancelled;
    private static Result ClosedGoal() => Result.Fail(AppError.Conflict("Завершённую или удалённую цель нельзя изменять."));

    private static void RefreshReviewStatus(LearningGoal goal) =>
        goal.Status = goal.Tasks.Count > 0 && goal.Tasks.All(x => x.Status == GoalTaskStatus.Completed)
            ? GoalStatus.PendingReview : GoalStatus.Active;
}
