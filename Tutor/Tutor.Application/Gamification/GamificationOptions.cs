namespace Tutor.Application.Gamification;

public sealed class GamificationOptions
{
    public const string Section = "Gamification";

    public StudentGamificationOptions Student { get; init; } = new();
    public MentorGamificationOptions Mentor { get; init; } = new();
}

public sealed class StudentGamificationOptions
{
    public int SessionPoints { get; init; }
    public int TaskPoints { get; init; }
    public int GoalPoints { get; init; }
    public int QuestionnairePoints { get; init; }
}

public sealed class MentorGamificationOptions
{
    public int SessionPoints { get; init; }
    public int StudentGoalPoints { get; init; }
    public int QuestionnairePoints { get; init; }
}
