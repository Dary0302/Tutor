namespace Tutor.Domain.Questionnaires;

public sealed class Questionnaire
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string City { get; set; }
    public required string Education { get; set; }
    public required string GoalDescription { get; set; }
    public CommunicationFormat CommunicationFormat { get; set; }
    public int MonthsNumberMin { get; set; }
    public int MonthsNumberMax { get; set; }
    public int HoursNumberWeek { get; set; }
    public required Guid UserId { get; set; }
}
