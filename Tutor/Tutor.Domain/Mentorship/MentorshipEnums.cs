namespace Tutor.Domain.Mentorship;

public enum RequestStatus
{
    Pending,
    Accepted,
    Declined,
    Cancelled
}

public enum PairStatus
{
    Active,
    Completed,
    Terminated
}

public enum PairEndReason
{
    GoalsAchieved,
    ParticipantRequest,
    Other
}

public enum GoalStatus
{
    Active,
    Completed,
    Cancelled
}

public enum GoalTaskStatus
{
    Pending,
    InProgress,
    Completed
}

public enum SessionStatus
{
    Scheduled,
    Completed,
    Cancelled
}

public enum SessionCancellationReason
{
    StudentRequest,
    TeacherRequest,
    Other
}
