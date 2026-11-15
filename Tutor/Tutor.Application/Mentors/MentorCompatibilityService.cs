using System.Linq.Expressions;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Mentors;

public sealed class MentorCompatibilityService : IMentorCompatibilityService
{
    private const decimal SkillsWeight = 40m;
    private const decimal DaysWeight = 20m;
    private const decimal DurationWeight = 20m;
    private const decimal HoursWeight = 10m;
    private const decimal CommunicationWeight = 10m;

    public Expression<Func<Questionnaire, MentorMatch>> CreateProjection(MentorPreferences? preferences)
    {
        if (preferences is null)
            return mentor => new MentorMatch { Mentor = mentor, Compatibility = null };

        var skills = preferences.SkillIds.Distinct().ToArray();
        var days = preferences.DaysOfWeek.Distinct().ToArray();
        var totalWeight = DurationWeight + HoursWeight + CommunicationWeight +
            (skills.Length > 0 ? SkillsWeight : 0m) + (days.Length > 0 ? DaysWeight : 0m);
        var skillCount = Math.Max(1, skills.Length);
        var dayCount = Math.Max(1, days.Length);

        return mentor => new MentorMatch
        {
            Mentor = mentor,
            Compatibility = Math.Floor(100m / totalWeight * (
                SkillsWeight * mentor.Skills.Count(s => skills.Contains(s.SkillId)) / skillCount +
                DaysWeight * mentor.Availability.Where(a => days.Contains(a.DayOfWeek))
                    .Select(a => a.DayOfWeek).Distinct().Count() / dayCount +
                (mentor.MonthsNumberMin <= preferences.MaxMonths && mentor.MonthsNumberMax >= preferences.MinMonths ? DurationWeight : 0m) +
                (mentor.HoursNumberWeek >= preferences.HoursPerWeek ? HoursWeight : 0m) +
                (mentor.CommunicationFormat == preferences.CommunicationFormat ||
                 mentor.CommunicationFormat == CommunicationFormat.Hybrid ||
                 preferences.CommunicationFormat == CommunicationFormat.Hybrid ? CommunicationWeight : 0m)) + 0.5m)
        };
    }
}
