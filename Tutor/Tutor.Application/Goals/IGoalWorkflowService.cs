using FluentResults;
using Tutor.Domain.Mentorship;

namespace Tutor.Application.Goals;

public interface IGoalWorkflowService
{
    Result AddTask(LearningGoal goal, GoalTask task);
    Result RemoveTask(LearningGoal goal, Guid taskId);
    Result SetTaskCompletion(LearningGoal goal, Guid taskId, bool completed);
    Result Confirm(LearningGoal goal);
    Result Cancel(LearningGoal goal);
}
