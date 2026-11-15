using Microsoft.Extensions.DependencyInjection;
using Tutor.Application.Article;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tutor.Application.Goals;
using Tutor.Application.Auth;
using Tutor.Application.Mentors;
using Tutor.Application.Questionnaires;

namespace Tutor.Application.Configurations;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        return services.AddScoped<AuthService>()
            .AddScoped<ArticleService>()
            .AddScoped<IGoalService, GoalService>()
            .AddScoped<IGoalProgressService, GoalProgressService>()
            .AddScoped<IGoalWorkflowService, GoalWorkflowService>()
            .AddScoped<IQuestionnaireService, QuestionnaireService>()
            .AddScoped<IMentorSearchService, MentorSearchService>()
            .AddScoped<IMentorCompatibilityService, MentorCompatibilityService>()
            .AddAutoMapper(_ => { }, typeof(QuestionnaireProfile));
    }
}
