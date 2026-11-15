using System.Linq.Expressions;
using Tutor.Domain.Questionnaires;

namespace Tutor.Application.Mentors;

public interface IMentorCompatibilityService
{
    Expression<Func<Questionnaire, MentorMatch>> CreateProjection(MentorPreferences? preferences);
}
